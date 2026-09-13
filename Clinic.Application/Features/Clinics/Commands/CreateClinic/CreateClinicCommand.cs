using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Clinics.Commands.CreateClinic;

public record CreateClinicCommand : IRequest<Result<int>>
{
    public string Name { get; init; } = string.Empty;
    public List<string> SpecializationNames { get; init; } = new();
}