using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Patients.Queries.GetCurrentPatient;

public record GetCurrentPatientQuery : IRequest<Result<CurrentPatientDto>>;

public record CurrentPatientDto(int Id, string FullName);