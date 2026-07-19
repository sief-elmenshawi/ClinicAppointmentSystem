using Clinic.Domain.Enums;

namespace Clinic.Application.Features.Appointments.Queries;

public record AppointmentDto(
    int Id,
    string DoctorName,
    string PatientName,
    DateTime AppointmentDateTime,
    AppointmentStatus Status,
    string? Notes);