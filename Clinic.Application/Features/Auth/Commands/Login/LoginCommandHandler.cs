using Clinic.Application.Common;
using Clinic.Application.Features.Auth.Commands.Login;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using MediatR;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IApplicationDbContext _context;

    public LoginCommandHandler(
        IIdentityService identityService, IJwtTokenGenerator jwtTokenGenerator, IApplicationDbContext context)
    {
        _identityService = identityService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _context = context;
    }

    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, roles, error) = await _identityService.ValidateCredentialsAsync(
            request.Email, request.Password);

        if (!succeeded)
            return Result<AuthResultDto>.Failure(error ?? "Login failed.");

        var accessToken = _jwtTokenGenerator.GenerateToken(userId!, request.Email, roles!);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            UserId = userId!,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Result<AuthResultDto>.Success(new AuthResultDto(accessToken, refreshToken));
    }
}