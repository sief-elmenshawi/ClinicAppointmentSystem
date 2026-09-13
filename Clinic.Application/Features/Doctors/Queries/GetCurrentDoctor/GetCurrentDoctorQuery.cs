using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Doctors.Queries.GetCurrentDoctor;

public record GetCurrentDoctorQuery : IRequest<Result<CurrentDoctorDto>>;

public record CurrentDoctorDto(int Id, string FullName);