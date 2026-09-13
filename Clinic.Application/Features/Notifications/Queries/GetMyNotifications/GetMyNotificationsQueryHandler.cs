using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, Result<NotificationsResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyNotificationsQueryHandler(
        IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<NotificationsResultDto>> Handle(
        GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            return Result<NotificationsResultDto>.Failure("Unauthorized.", ErrorType.Forbidden);

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.ApplicationUserId == currentUserId, cancellationToken);

        if (doctor is null)
            return Result<NotificationsResultDto>.Failure("Doctor profile not found.", ErrorType.NotFound);

        var items = await _context.Notifications
            .Where(n => n.DoctorId == doctor.Id)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto(
                n.Id, n.Type, n.Message, n.AppointmentId, n.IsRead, n.CreatedAt))
            .ToListAsync(cancellationToken);

        var unreadCount = await _context.Notifications
            .CountAsync(n => n.DoctorId == doctor.Id && !n.IsRead, cancellationToken);

        return Result<NotificationsResultDto>.Success(new NotificationsResultDto(unreadCount, items));
    }
}