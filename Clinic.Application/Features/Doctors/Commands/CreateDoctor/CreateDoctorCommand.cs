using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Doctors.Commands.CreateDoctor;

public record CreateDoctorCommand : IRequest<Result<int>>
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int SpecializationId { get; init; }
    public decimal ConsultationFee { get; init; }
}