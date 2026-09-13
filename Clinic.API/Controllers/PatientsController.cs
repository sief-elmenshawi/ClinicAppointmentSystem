using Clinic.Application.Features.Patients.Commands.CreatePatient;
using Clinic.Application.Features.Patients.Queries.GetCurrentPatient;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePatientCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToHttpResult();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await _mediator.Send(new GetCurrentPatientQuery());
        return result.ToHttpResult();
    }
}