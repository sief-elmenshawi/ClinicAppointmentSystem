using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Notifications.Queries.GetMyNotifications;

public record GetMyNotificationsQuery : IRequest<Result<NotificationsResultDto>>;