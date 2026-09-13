using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Application.Features.Specializations.Queries.GetAllSpecializations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Clinics.Queries.GetAllClinics;

public class GetAllClinicsQueryHandler : IRequestHandler<GetAllClinicsQuery, Result<List<ClinicDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllClinicsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ClinicDto>>> Handle(
        GetAllClinicsQuery request, CancellationToken cancellationToken)
    {
        var departments = await _context.Departments
            .AsNoTracking()
            .Include(d => d.Specializations)
            .OrderBy(d => d.Id)
            .ToListAsync(cancellationToken);

        var result = departments.Select(department => new ClinicDto(
            department.Id,
            department.Name,
            department.Specializations
                .OrderBy(s => s.Name)
                .Select(s => new SpecializationDto(s.Id, s.Name))
                .ToList()))
            .ToList();

        return Result<List<ClinicDto>>.Success(result);
    }
}