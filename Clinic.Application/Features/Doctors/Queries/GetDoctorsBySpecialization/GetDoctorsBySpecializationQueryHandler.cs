using Clinic.Application.Common;
using Clinic.Application.Common.Specifications;
using Clinic.Application.Features.Doctors.Specifications;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Doctors.Queries.GetDoctorsBySpecialization;

public class GetDoctorsBySpecializationQueryHandler
    : IRequestHandler<GetDoctorsBySpecializationQuery, Result<List<DoctorDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetDoctorsBySpecializationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<DoctorDto>>> Handle(
        GetDoctorsBySpecializationQuery request, CancellationToken cancellationToken)
    {
        var spec = new DoctorsBySpecializationSpec(request.SpecializationId);

        var doctors = await SpecificationEvaluator<Doctor>
            .GetQuery(_context.Doctors.AsNoTracking(), spec)
            .Select(d => new DoctorDto(d.Id, d.FullName, d.Specialization.Name, d.ConsultationFee))
            .ToListAsync(cancellationToken);

        return Result<List<DoctorDto>>.Success(doctors);
    }
}