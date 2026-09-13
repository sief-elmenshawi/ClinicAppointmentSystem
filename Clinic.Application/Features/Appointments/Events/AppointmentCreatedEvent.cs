using MediatR;

namespace Clinic.Application.Features.Appointments.Events;

public record AppointmentCreatedEvent(
    int AppointmentId,
    int DoctorId,
    string PatientName,
    string PatientEmail,
    DateTime AppointmentDateTime)
    : INotification;