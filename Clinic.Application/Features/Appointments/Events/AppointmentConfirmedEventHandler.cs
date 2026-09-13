using Clinic.Application.Interfaces;
using MediatR;

namespace Clinic.Application.Features.Appointments.Events;

public class AppointmentConfirmedEventHandler : INotificationHandler<AppointmentConfirmedEvent>
{
    private readonly IEmailService _emailService;

    public AppointmentConfirmedEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(AppointmentConfirmedEvent notification, CancellationToken cancellationToken)
    {
        await _emailService.SendAsync(
            notification.PatientEmail,
            "Appointment Confirmed",
            $"Dear {notification.PatientName}, your appointment with {notification.DoctorName} on {notification.AppointmentDateTime:f} has been confirmed.");
    }
}