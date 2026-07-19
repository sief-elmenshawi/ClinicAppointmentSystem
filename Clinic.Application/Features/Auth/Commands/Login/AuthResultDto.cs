namespace Clinic.Application.Features.Auth.Commands.Login;

public record AuthResultDto(string AccessToken, string RefreshToken);