using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Doctors.Queries.GetCurrentDoctor;

public class GetCurrentDoctorQueryHandler
    : IRequestHandler<GetCurrentDoctorQuery, Result<CurrentDoctorDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentDoctorQueryHandler(
        IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CurrentDoctorDto>> Handle(
        GetCurrentDoctorQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.ApplicationUserId == userId, cancellationToken);

        if (doctor is null)
            return Result<CurrentDoctorDto>.Failure("No doctor profile linked to this account.", ErrorType.NotFound);

        return Result<CurrentDoctorDto>.Success(new CurrentDoctorDto(doctor.Id, doctor.FullName));
    }
}