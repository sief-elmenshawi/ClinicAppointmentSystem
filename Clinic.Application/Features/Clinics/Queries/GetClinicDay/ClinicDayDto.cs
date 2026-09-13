using Clinic.Application.Common;

namespace Clinic.Application.Features.Clinics.Queries.GetClinicDay;

public record ClinicDayDto(
    int ClinicId,
    string ClinicName,
    List<DoctorDayStatusDto> Doctors);