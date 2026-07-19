using Clinic.Application.Common.Specifications;
using Clinic.Domain.Entities;

namespace Clinic.Application.Features.Doctors.Specifications;

public class DoctorsBySpecializationSpec : BaseSpecification<Doctor>
{
    public DoctorsBySpecializationSpec(int specializationId)
    {
        AddCriteria(d => d.SpecializationId == specializationId);
        AddInclude(d => d.Specialization);
        AddOrderBy(d => d.FullName);

    }

}