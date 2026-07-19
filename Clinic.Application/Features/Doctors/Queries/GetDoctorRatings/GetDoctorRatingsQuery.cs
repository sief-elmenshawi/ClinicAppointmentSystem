using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Doctors.Queries.GetDoctorRatings;

public record GetDoctorRatingsQuery(int DoctorId) : IRequest<Result<DoctorRatingsSummaryDto>>;