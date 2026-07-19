using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<Result<int>>
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
}