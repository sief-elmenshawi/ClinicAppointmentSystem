using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Clinics.Queries.GetAllClinics;

public record GetAllClinicsQuery : IRequest<Result<List<ClinicDto>>>;