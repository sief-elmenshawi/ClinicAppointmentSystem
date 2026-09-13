namespace Clinic.Application.Features.Notifications.Queries.GetMyNotifications;

public record NotificationDto(
    int Id,
    string Type,
    string Message,
    int? AppointmentId,
    bool IsRead,
    DateTime CreatedAt);

public record NotificationsResultDto(int UnreadCount, List<NotificationDto> Items);