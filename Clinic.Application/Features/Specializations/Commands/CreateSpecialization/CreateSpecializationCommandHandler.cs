using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using static Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions;
using MediatR;

namespace Clinic.Application.Features.Specializations.Commands.CreateSpecialization;

public class CreateSpecializationCommandHandler
    : IRequestHandler<CreateSpecializationCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public CreateSpecializationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(
        CreateSpecializationCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Specializations
            .AnyAsync(s => s.Name == request.Name, cancellationToken);

        if (exists)
            return Result<int>.Failure("Specialization already exists.");

        var specialization = new Specialization { Name = request.Name };

        _context.Specializations.Add(specialization);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(specialization.Id);
    }
}