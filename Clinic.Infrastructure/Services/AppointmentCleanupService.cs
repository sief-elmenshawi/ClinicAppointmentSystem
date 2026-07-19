using Clinic.Application.Interfaces;
using Clinic.Domain.Enums;
using Clinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clinic.Infrastructure.Services;

public class AppointmentCleanupService : IAppointmentCleanupService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AppointmentCleanupService> _logger;

    public AppointmentCleanupService(ApplicationDbContext context, ILogger<AppointmentCleanupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CancelStalePendingAppointmentsAsync()
    {
        var cutoff = DateTime.UtcNow.AddHours(-1);

        var staleAppointments = await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Pending && a.CreatedAt <= cutoff)
            .ToListAsync();

        if (staleAppointments.Count == 0)
            return;

        foreach (var appointment in staleAppointments)
        {
            appointment.Status = AppointmentStatus.Cancelled;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cancelled {Count} stale pending appointments", staleAppointments.Count);
    }
}