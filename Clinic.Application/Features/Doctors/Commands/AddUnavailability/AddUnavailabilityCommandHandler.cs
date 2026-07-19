using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using static Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions;
using MediatR;

namespace Clinic.Application.Features.Doctors.Commands.AddUnavailability;

public class AddUnavailabilityCommandHandler : IRequestHandler<AddUnavailabilityCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public AddUnavailabilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(AddUnavailabilityCommand request, CancellationToken cancellationToken)
    {
        var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == request.DoctorId, cancellationToken);
        if (!doctorExists)
            return Result<int>.Failure("Doctor not found.");

        var alreadyExists = await _context.DoctorUnavailabilities
            .AnyAsync(u => u.DoctorId == request.DoctorId && u.Date == request.Date, cancellationToken);
        if (alreadyExists)
            return Result<int>.Failure("This date is already marked as unavailable.");

        var unavailability = new DoctorUnavailability
        {
            DoctorId = request.DoctorId,
            Date = request.Date,
            Reason = request.Reason
        };

        _context.DoctorUnavailabilities.Add(unavailability);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(unavailability.Id);
    }
}