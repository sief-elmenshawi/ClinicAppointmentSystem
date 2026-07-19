using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Doctors.Queries.GetDoctorsBySpecialization;

public record GetDoctorsBySpecializationQuery(int SpecializationId) : IRequest<Result<List<DoctorDto>>>;