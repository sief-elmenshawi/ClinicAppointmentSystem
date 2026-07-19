using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Appointments.Queries.GetPatientAppointments;

public class GetPatientAppointmentsQueryHandler
    : IRequestHandler<GetPatientAppointmentsQuery, Result<List<AppointmentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetPatientAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AppointmentDto>>> Handle(
        GetPatientAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.PatientId == request.PatientId)
            .OrderBy(a => a.AppointmentDateTime)
            .Select(a => new AppointmentDto(
                a.Id,
                a.Doctor.FullName,
                a.Patient.FullName,
                a.AppointmentDateTime,
                a.Status,
                a.Notes))
            .ToListAsync(cancellationToken);

        return Result<List<AppointmentDto>>.Success(appointments);
    }
}