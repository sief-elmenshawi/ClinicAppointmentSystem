using Clinic.Application.Features.Specializations.Commands.CreateSpecialization;
using Clinic.Application.Features.Specializations.Queries.GetAllSpecializations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecializationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SpecializationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSpecializationCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.Error);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetAllSpecializationsQuery(pageNumber, pageSize));
        return Ok(result.Value);
    }
}