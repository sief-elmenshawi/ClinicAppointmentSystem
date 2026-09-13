using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Infrastructure.Identity;
using Clinic.Infrastructure.Persistence;
using Clinic.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Clinic.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextPool<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            // السماح بالأسماء العربية كـ Username (أسماء الدكاترة في الـ Seed)
            options.User.AllowedUserNameCharacters = null;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddHttpContextAccessor(); 
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // JWT Settings binding
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Fail-fast: ممنوع تشغيل التطبيق بـ secret مفضوح/placeholder أو أقصر من 32 حرف.
        // الـ secret الحقيقي لازم يتضبط برة الريبو: JwtSettings__Secret (Environment Variable)
        // أو user-secrets وقت التطوير المحلي.
        ValidateJwtSecret(jwtSettings.Secret, IsDevelopment(configuration));

        // JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });
        services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));

        services.AddHangfireServer();

        services.AddScoped<IAppointmentCleanupService, AppointmentCleanupService>();
        services.AddScoped<IRefreshTokenCleanupService, RefreshTokenCleanupService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsSender, SmsSender>();
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(name: "database");

        return services;
    }

    private static void ValidateJwtSecret(string secret, bool isDevelopment)
    {
        var isKnownPlaceholder = string.IsNullOrEmpty(secret)
                                 || string.Equals(secret, "CHANGE_ME_IN_PRODUCTION", StringComparison.Ordinal)
                                 || secret.Length < 32;

        // أي بيئة غير الـ Development ممنوع فيها secret متسجل في الريبو نفسه — حتى لو طويل —
        // لأن أي قيمة موجودة في appsettings.*.json المتسجلة = معروفة publicly.
        // الـ dev secret بيحمل علامة "DevOnly" وبيتسمح بيها بس لما تكون فعلاً في Development.
        if (!isDevelopment && secret.Contains("DevOnly", StringComparison.OrdinalIgnoreCase))
            isKnownPlaceholder = true;

        if (isKnownPlaceholder)
        {
            throw new InvalidOperationException(
                "JwtSettings:Secret لازم يكون 32+ حرف عشوائي وsecret جديد غير معروف publicly. " +
                "في أي بيئة غير Development ممنوع استخدام أي secret متسجل في الريبو. " +
                "اضبطه برة الريبو بـ Environment Variable: JwtSettings__Secret " +
                "(أو dotnet user-secrets في التطوير المحلي).");
        }
    }

    private static bool IsDevelopment(IConfiguration configuration)
    {
        var environment = configuration["environment"]
                          ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                          ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        return string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
    }
}