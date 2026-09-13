using Clinic.Application.Features.Admin.Queries.GetAdminDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET /api/admin/dashboard?date=2026-09-12
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly? date)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var result = await _mediator.Send(new GetAdminDashboardQuery(targetDate));
        return result.ToHttpResult();
    }
}