using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Admin.Queries.GetAdminDashboard;

public record GetAdminDashboardQuery(DateOnly Date) : IRequest<Result<AdminDashboardDto>>;