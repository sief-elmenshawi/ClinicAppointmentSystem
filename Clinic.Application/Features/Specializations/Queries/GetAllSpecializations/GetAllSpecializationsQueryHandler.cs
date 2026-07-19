using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Specializations.Queries.GetAllSpecializations;

public class GetAllSpecializationsQueryHandler
    : IRequestHandler<GetAllSpecializationsQuery, Result<PagedResult<SpecializationDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllSpecializationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<SpecializationDto>>> Handle(
        GetAllSpecializationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Specializations.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(s => s.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SpecializationDto(s.Id, s.Name))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<SpecializationDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<SpecializationDto>>.Success(result);
    }
}