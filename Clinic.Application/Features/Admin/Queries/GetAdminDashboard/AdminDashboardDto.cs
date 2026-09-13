using Clinic.Application.Common;

namespace Clinic.Application.Features.Admin.Queries.GetAdminDashboard;

public record AdminDashboardDto(
    string Date,
    DashboardOverviewDto Overview,
    List<WaitingListItemDto> WaitingList,
    List<DoctorDayStatusDto> DoctorsToday);