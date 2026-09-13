using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Clinics.Commands.CreateClinic;

public class CreateClinicCommandHandler : IRequestHandler<CreateClinicCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public CreateClinicCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateClinicCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var exists = await _context.Departments.AnyAsync(d => d.Name == name, cancellationToken);
        if (exists)
            return Result<int>.Failure("Clinic name already exists.", ErrorType.Conflict);

        var department = new Department
        {
            Name = name,
            Specializations = request.SpecializationNames
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => new Specialization { Name = s.Trim() })
                .ToList()
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(department.Id);
    }
}