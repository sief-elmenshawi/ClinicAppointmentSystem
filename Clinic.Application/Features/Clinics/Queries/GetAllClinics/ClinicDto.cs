using Clinic.Application.Features.Specializations.Queries.GetAllSpecializations;

namespace Clinic.Application.Features.Clinics.Queries.GetAllClinics;

public record ClinicDto(int Id, string Name, List<SpecializationDto> Specializations);