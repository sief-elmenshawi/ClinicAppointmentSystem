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
        // الهدف: تحرير الـ Slots القديمة بس — نلغي الحجوزات Pending اللي ميعادها فات
        // (مريض حجز وهو لم يأتِ والطبيب لم يؤكد)؛ لا نلغي حجز لأسبوع قادم لم يُؤكد بعد.
        var now = DateTime.Now; // الـ AppointmentDateTime وقت محلي

        var cancelled = await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Pending && a.AppointmentDateTime <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, AppointmentStatus.Cancelled));

        if (cancelled > 0)
            _logger.LogInformation("Cancelled {Count} stale pending appointments", cancelled);
    }
}