using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Appointments.Queries.GetAllAppointments;

public class GetAllAppointmentsQueryHandler
    : IRequestHandler<GetAllAppointmentsQuery, Result<PagedResult<AppointmentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<AppointmentDto>>> Handle(
        GetAllAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments.AsNoTracking();

        if (request.DoctorId is int doctorId)
            query = query.Where(a => a.DoctorId == doctorId);

        if (request.Status is AppointmentStatus status)
            query = query.Where(a => a.Status == status);

        if (request.ClinicId is int clinicId)
            query = query.Where(a => a.Doctor.Specialization.DepartmentId == clinicId);

        if (request.FromDate is DateOnly from)
            query = query.Where(a => a.AppointmentDateTime >= from.ToDateTime(TimeOnly.MinValue));

        if (request.ToDate is DateOnly to)
            query = query.Where(a => a.AppointmentDateTime < to.ToDateTime(TimeOnly.MinValue).AddDays(1));

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