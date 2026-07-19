using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Specializations.Queries.GetAllSpecializations;

public record GetAllSpecializationsQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<Result<PagedResult<SpecializationDto>>>;