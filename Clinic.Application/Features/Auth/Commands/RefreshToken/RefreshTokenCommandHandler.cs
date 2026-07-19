using Clinic.Application.Common;
using Clinic.Application.Features.Auth.Commands.Login;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator, IIdentityService identityService)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _identityService = identityService;
    }

    public async Task<Result<AuthResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            return Result<AuthResultDto>.Failure("Invalid or expired refresh token.");

        var email = await _identityService.GetUserEmailAsync(storedToken.UserId);
        var roles = await _identityService.GetRolesByUserIdAsync(storedToken.UserId);

        var newAccessToken = _jwtTokenGenerator.GenerateToken(storedToken.UserId, email!, roles);

        return Result<AuthResultDto>.Success(new AuthResultDto(newAccessToken, storedToken.Token));
    }
}