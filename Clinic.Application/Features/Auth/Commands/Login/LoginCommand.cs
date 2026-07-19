using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResultDto>>;