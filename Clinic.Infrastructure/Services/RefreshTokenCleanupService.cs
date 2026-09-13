using Clinic.Application.Interfaces;
using Clinic.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Clinic.Infrastructure.Services;

public class RefreshTokenCleanupService : IRefreshTokenCleanupService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RefreshTokenCleanupService> _logger;

    public RefreshTokenCleanupService(ApplicationDbContext context, ILogger<RefreshTokenCleanupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        // نتخلص من الـ RefreshTokens التي انتهت صلاحيتها أو التي أُلغيت بعد الدوران
        // لمنع تراكم غير محدود في الجدول (نمو غير مراقب).
        var now = DateTime.Now;

        var deleted = await _context.RefreshTokens
            .Where(t => t.ExpiresAt <= now || t.IsRevoked)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
            _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", deleted);
    }
}