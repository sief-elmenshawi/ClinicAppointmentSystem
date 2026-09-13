using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Doctors.Queries.GetDoctorRatings;

public class GetDoctorRatingsQueryHandler
    : IRequestHandler<GetDoctorRatingsQuery, Result<DoctorRatingsSummaryDto>>
{
    private const int MaxRatingsListed = 50;

    private readonly IApplicationDbContext _context;

    public GetDoctorRatingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DoctorRatingsSummaryDto>> Handle(
        GetDoctorRatingsQuery request, CancellationToken cancellationToken)
    {
        // المتوسط والعدد في SQL بدون تحميل كل الصفوف
        var totalCount = await _context.DoctorRatings
            .AsNoTracking()
            .Where(r => r.DoctorId == request.DoctorId)
            .CountAsync(cancellationToken);

        var average = totalCount > 0
            ? await _context.DoctorRatings
                .AsNoTracking()
                .Where(r => r.DoctorId == request.DoctorId)
                .AverageAsync(r => (double)r.Score, cancellationToken)
            : 0;

        var ratings = await _context.DoctorRatings
            .AsNoTracking()
            .Where(r => r.DoctorId == request.DoctorId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(MaxRatingsListed)
            .Select(r => new DoctorRatingDto(r.Patient.FullName, r.Score, r.Comment, r.CreatedAt))
            .ToListAsync(cancellationToken);

        var summary = new DoctorRatingsSummaryDto(
            AverageScore: Math.Round(average, 2),
            TotalRatings: totalCount,
            Ratings: ratings);

        return Result<DoctorRatingsSummaryDto>.Success(summary);
    }
}