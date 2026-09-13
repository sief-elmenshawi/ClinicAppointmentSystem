using Clinic.Application.Common;
using Clinic.Domain.Enums;
using MediatR;

namespace Clinic.Application.Features.Appointments.Queries.GetAllAppointments;

public record GetAllAppointmentsQuery(
    DateOnly? FromDate,
    DateOnly? ToDate,
    int? ClinicId,
    int? DoctorId,
    AppointmentStatus? Status,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<Result<PagedResult<AppointmentDto>>>;