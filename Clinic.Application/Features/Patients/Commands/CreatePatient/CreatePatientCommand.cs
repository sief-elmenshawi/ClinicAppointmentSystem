using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Patients.Commands.CreatePatient;

public record CreatePatientCommand : IRequest<Result<int>>
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string? MedicalHistory { get; init; }
}