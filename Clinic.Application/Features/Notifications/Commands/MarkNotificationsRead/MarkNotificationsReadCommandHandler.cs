using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Notifications.Commands.MarkNotificationsRead;

public class MarkNotificationsReadCommandHandler
    : IRequestHandler<MarkNotificationsReadCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MarkNotificationsReadCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(
        MarkNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            return Result<bool>.Failure("Unauthorized.", ErrorType.Forbidden);

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.ApplicationUserId == currentUserId, cancellationToken);

        if (doctor is null)
            return Result<bool>.Failure("Doctor profile not found.", ErrorType.NotFound);

        var updated = await _context.Notifications
            .Where(n => n.DoctorId == doctor.Id && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, DateTime.UtcNow),
                cancellationToken);

        return Result<bool>.Success(true);
    }
}