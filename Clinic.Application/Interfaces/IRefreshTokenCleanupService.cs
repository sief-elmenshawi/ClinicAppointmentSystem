namespace Clinic.Application.Interfaces;

public interface IRefreshTokenCleanupService
{
    Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}