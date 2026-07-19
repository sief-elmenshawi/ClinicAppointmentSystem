namespace Clinic.Application.Features.Doctors.Queries.GetDoctorRatings;

public record DoctorRatingDto(string PatientName, int Score, string? Comment, DateTime CreatedAt);

public record DoctorRatingsSummaryDto(double AverageScore, int TotalRatings, List<DoctorRatingDto> Ratings);