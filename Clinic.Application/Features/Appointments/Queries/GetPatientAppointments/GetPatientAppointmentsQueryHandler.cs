using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Appointments.Queries.GetPatientAppointments;

public class GetPatientAppointmentsQueryHandler
    : IRequestHandler<GetPatientAppointmentsQuery, Result<List<AppointmentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPatientAppointmentsQueryHandler(
        IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<AppointmentDto>>> Handle(
        GetPatientAppointmentsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            var isOwner = await _context.Patients
                .AnyAsync(p => p.Id == request.PatientId
                               && p.ApplicationUserId == _currentUserService.UserId,
                    cancellationToken);

            if (!isOwner)
                return Result<List<AppointmentDto>>.Failure(
                    "You are not authorized to view these appointments.", ErrorType.Forbidden);
        }

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