using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Appointments.Queries.GetDoctorAppointments;

public class GetDoctorAppointmentsQueryHandler
    : IRequestHandler<GetDoctorAppointmentsQuery, Result<PagedResult<AppointmentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetDoctorAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<AppointmentDto>>> Handle(
        GetDoctorAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == request.DoctorId);

        if (request.Date is not null)
        {
            var dayStart = request.Date.Value.ToDateTime(TimeOnly.MinValue);
            var dayEnd = dayStart.AddDays(1);
            query = query.Where(a => a.AppointmentDateTime >= dayStart && a.AppointmentDateTime < dayEnd);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(a => a.AppointmentDateTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new AppointmentDto(
                a.Id, a.Doctor.FullName, a.Patient.FullName,
                a.AppointmentDateTime, a.Status, a.Notes))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<AppointmentDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<AppointmentDto>>.Success(result);
    }
}