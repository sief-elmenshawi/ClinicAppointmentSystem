using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Doctors.Queries.GetAllDoctors;

public record GetAllDoctorsQuery(int PageNumber = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<DoctorListItemDto>>>;

public record DoctorListItemDto(
    int Id,
    string FullName,
    string SpecializationName,
    decimal ConsultationFee);