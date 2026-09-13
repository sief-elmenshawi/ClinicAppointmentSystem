using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Doctors.Queries.GetAllDoctors;

public class GetAllDoctorsQueryHandler
    : IRequestHandler<GetAllDoctorsQuery, Result<PagedResult<DoctorListItemDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllDoctorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<DoctorListItemDto>>> Handle(
        GetAllDoctorsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Doctors.AsNoTracking()
            .Include(d => d.Specialization);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(d => d.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new DoctorListItemDto(
                d.Id, d.FullName, d.Specialization.Name, d.ConsultationFee))
            .ToListAsync(cancellationToken);

        var result = new PagedResult<DoctorListItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<DoctorListItemDto>>.Success(result);
    }
}