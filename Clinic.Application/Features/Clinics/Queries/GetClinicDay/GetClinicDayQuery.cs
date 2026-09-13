using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Clinics.Queries.GetClinicDay;

public record GetClinicDayQuery(
    DateOnly Date,
    int? ClinicId,
    int? SpecializationId) : IRequest<Result<List<ClinicDayDto>>>;