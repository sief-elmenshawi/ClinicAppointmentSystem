using Clinic.API;
using Clinic.Application;
using Clinic.Application.Interfaces;
using Clinic.Infrastructure;
using Clinic.Infrastructure.Persistence.Seed;
using Hangfire;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Clinic API");

    var builder = WebApplication.CreateBuilder(args);

    // ── Logging ──────────────────────────────────
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    // ── Services ─────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddProblemDetails();

    // ── Response Compression (GZip/Deflate للجداول الكبيرة من Browsers/ـ Mobile) ──
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    });

    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices();

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter("AuthPolicy", opt =>
        {
            opt.PermitLimit = 5;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 0;
        });
    });

    // ══ CORS ══
    // الـ origins المُسمح بيها بتتقرا من الـ config (Cors:AllowedOrigins) عشان تقدر
    // ترفع الفرونت على أي دومين (Vercel/Netlify) وتضيفه في الـ Production من غير تعديل كود.
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    var app = builder.Build();

    // ── Middleware Pipeline (الترتيب مهم جدًا) ───
    app.UseSerilogRequestLogging();
    app.UseMiddleware<Clinic.API.Middlewares.ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // HTTPS redirect معطّل في الـ Development حتى لا يكسر اتصال الواجهة عبر الـ proxy
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseResponseCompression();
    app.UseCors("Frontend");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");
    // لوحة الـ dashboard مش مفتوحة لأي حد — محتاجة Bearer token بيوزر عليه صلاحية Admin
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAdminAuthFilter() }
    });

    // ── Seed Data (Roles + Admin + Clinics/Doctors) ───
    // فقط في الـ Development — لا نزرع حساب Admin بكلمة سر معروفة في Production
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            await RoleSeeder.SeedRolesAsync(scope.ServiceProvider);
            await AdminSeeder.SeedAdminAsync(scope.ServiceProvider);
            await ClinicSeeder.SeedClinicsAndDoctorsAsync(scope.ServiceProvider);
        }
    }

    // ── Recurring Jobs (Hangfire) ──────────────────
    using (var jobScope = app.Services.CreateScope())
    {
        var recurringJobManager = jobScope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<IAppointmentCleanupService>(
            "cancel-stale-appointments",
            service => service.CancelStalePendingAppointmentsAsync(),
            Cron.Hourly);

        recurringJobManager.AddOrUpdate<IRefreshTokenCleanupService>(
            "cleanup-refresh-tokens",
            service => service.CleanupExpiredTokensAsync(),
            Cron.Daily);
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Clinic API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}