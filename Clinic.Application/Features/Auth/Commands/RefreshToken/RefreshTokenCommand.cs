using Clinic.Application.Common;
using Clinic.Application.Features.Auth.Commands.Login;
using MediatR;

namespace Clinic.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResultDto>>;