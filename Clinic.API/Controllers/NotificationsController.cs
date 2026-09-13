using Clinic.Application.Features.Notifications.Commands.MarkNotificationsRead;
using Clinic.Application.Features.Notifications.Queries.GetMyNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Doctor")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET /api/notifications
    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var result = await _mediator.Send(new GetMyNotificationsQuery());
        return result.ToHttpResult();
    }

    // PUT /api/notifications/read-all
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var result = await _mediator.Send(new MarkNotificationsReadCommand());
        return result.ToHttpActionResult();
    }
}