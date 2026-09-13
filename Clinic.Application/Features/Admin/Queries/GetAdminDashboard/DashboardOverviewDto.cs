namespace Clinic.Application.Features.Admin.Queries.GetAdminDashboard;

public record DashboardOverviewDto(
    int TotalAppointments,
    int PendingCount,
    int ConfirmedCount,
    int CompletedCount,
    int CancelledCount,
    int NoShowCount,
    int DoctorsScheduled,
    int DoctorsWorking,
    int DoctorsFree,
    int DoctorsAbsent,
    int ClinicsCount,
    int DoctorsCount,
    int PatientsCount,
    decimal TodayRevenue);