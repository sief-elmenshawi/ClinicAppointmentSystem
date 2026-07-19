using MediatR;

namespace Clinic.Application.Features.Appointments.Events;

public record AppointmentCreatedEvent(int AppointmentId, string PatientEmail, DateTime AppointmentDateTime)
    : INotification;