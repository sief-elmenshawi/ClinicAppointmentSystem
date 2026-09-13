using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Patients.Queries.GetCurrentPatient;

public class GetCurrentPatientQueryHandler
    : IRequestHandler<GetCurrentPatientQuery, Result<CurrentPatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentPatientQueryHandler(
        IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CurrentPatientDto>> Handle(
        GetCurrentPatientQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.ApplicationUserId == userId, cancellationToken);

        if (patient is null)
            return Result<CurrentPatientDto>.Failure("No patient profile linked to this account.", ErrorType.NotFound);

        return Result<CurrentPatientDto>.Success(new CurrentPatientDto(patient.Id, patient.FullName));
    }
}