namespace Clinic.Application.Features.Admin.Queries.GetAdminDashboard;

public record WaitingListItemDto(
    int Id,
    string PatientName,
    string DoctorName,
    string ClinicName,
    DateTime AppointmentDateTime,
    DateTime CreatedAt);