namespace Clinic.Application.Features.Doctors.Queries.GetDoctorsBySpecialization;

public record DoctorDto(int Id, string FullName, string SpecializationName, decimal ConsultationFee);