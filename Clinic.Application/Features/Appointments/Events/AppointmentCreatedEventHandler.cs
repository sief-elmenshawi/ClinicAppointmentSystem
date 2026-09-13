using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clinic.Application.Features.Appointments.Events;

public class AppointmentCreatedEventHandler : INotificationHandler<AppointmentCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ISmsSender _smsSender;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AppointmentCreatedEventHandler> _logger;

    public AppointmentCreatedEventHandler(
        IEmailService emailService,
        ISmsSender smsSender,
        IApplicationDbContext context,
        ILogger<AppointmentCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _smsSender = smsSender;
        _context = context;
        _logger = logger;
    }

    public async Task Handle(AppointmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        // تأكيد إلكتروني للمريض
        await _emailService.SendAsync(
            notification.PatientEmail,
            "Appointment Confirmation",
            $"Dear {notification.PatientName}, your appointment is scheduled for {notification.AppointmentDateTime:f}.");

        // معلومات إشعار لدكتور
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == notification.DoctorId, cancellationToken);

        if (doctor is null)
            return;

        var message =
            $"حجز جديد من المريض {notification.PatientName} يوم " +
            $"{notification.AppointmentDateTime:yyyy-MM-dd} الساعة {notification.AppointmentDateTime:HH:mm}";

        _context.Notifications.Add(new Notification
        {
            DoctorId = doctor.Id,
            AppointmentId = notification.AppointmentId,
            Type = "NewBooking",
            Message = message
        });

        await _context.SaveChangesAsync(cancellationToken);

        // SMS للدكتور على رقمه
        if (!string.IsNullOrWhiteSpace(doctor.PhoneNumber))
        {
            await _smsSender.SendAsync(doctor.PhoneNumber, message);
        }
        else
        {
            _logger.LogWarning("Doctor {DoctorId} has no phone number to receive booking SMS.", doctor.Id);
        }
    }
}