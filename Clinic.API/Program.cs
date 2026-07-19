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

    var app = builder.Build();

    // ── Middleware Pipeline (الترتيب مهم جدًا) ───
    app.UseSerilogRequestLogging();
    app.UseMiddleware<Clinic.API.Middlewares.ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");
    app.UseHangfireDashboard("/hangfire");

    // ── Seed Data (Roles + Admin) ─────────────────
    using (var scope = app.Services.CreateScope())
    {
        await RoleSeeder.SeedRolesAsync(scope.ServiceProvider);
        await AdminSeeder.SeedAdminAsync(scope.ServiceProvider);
    }

    // ── Recurring Jobs (Hangfire) ──────────────────
    using (var jobScope = app.Services.CreateScope())
    {
        var recurringJobManager = jobScope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<IAppointmentCleanupService>(
            "cancel-stale-appointments",
            service => service.CancelStalePendingAppointmentsAsync(),
            Cron.Hourly);
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