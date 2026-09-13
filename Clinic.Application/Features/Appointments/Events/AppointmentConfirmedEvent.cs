using MediatR;

namespace Clinic.Application.Features.Appointments.Events;

public record AppointmentConfirmedEvent(
    int AppointmentId,
    string DoctorName,
    string PatientName,
    string PatientEmail,
    DateTime AppointmentDateTime)
    : INotification;