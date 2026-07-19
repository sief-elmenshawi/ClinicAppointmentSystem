using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Doctors.Queries.GetDoctorRatings;

public class GetDoctorRatingsQueryHandler
    : IRequestHandler<GetDoctorRatingsQuery, Result<DoctorRatingsSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDoctorRatingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DoctorRatingsSummaryDto>> Handle(
        GetDoctorRatingsQuery request, CancellationToken cancellationToken)
    {
        var ratings = await _context.DoctorRatings
            .AsNoTracking()
            .Where(r => r.DoctorId == request.DoctorId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new DoctorRatingDto(r.Patient.FullName, r.Score, r.Comment, r.CreatedAt))
            .ToListAsync(cancellationToken);

        var summary = new DoctorRatingsSummaryDto(
            AverageScore: ratings.Count > 0 ? Math.Round(ratings.Average(r => r.Score), 2) : 0,
            TotalRatings: ratings.Count,
            Ratings: ratings);

        return Result<DoctorRatingsSummaryDto>.Success(summary);
    }
}