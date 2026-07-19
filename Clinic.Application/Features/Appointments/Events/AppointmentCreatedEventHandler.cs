using Clinic.Application.Interfaces;
using MediatR;

namespace Clinic.Application.Features.Appointments.Events;

public class AppointmentCreatedEventHandler : INotificationHandler<AppointmentCreatedEvent>
{
    private readonly IEmailService _emailService;

    public AppointmentCreatedEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(AppointmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _emailService.SendAsync(
            notification.PatientEmail,
            "Appointment Confirmation",
            $"Your appointment is scheduled for {notification.AppointmentDateTime:f}.");
    }
}