using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Appointments.Queries.GetDoctorAppointments;

public record GetDoctorAppointmentsQuery(
    int DoctorId, DateOnly? Date, int PageNumber = 1, int PageSize = 10)
    : IRequest<Result<PagedResult<AppointmentDto>>>;