using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Notifications.Commands.MarkNotificationsRead;

public record MarkNotificationsReadCommand : IRequest<Result<bool>>;