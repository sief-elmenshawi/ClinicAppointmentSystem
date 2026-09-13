using Clinic.Application.Features.Clinics.Commands.CreateClinic;
using Clinic.Application.Features.Clinics.Queries.GetAllClinics;
using Clinic.Application.Features.Clinics.Queries.GetClinicDay;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClinicsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET /api/clinics
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllClinicsQuery());
        return Ok(result.Value);
    }

    // POST /api/clinics
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateClinicCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToHttpResult();
    }

    // GET /api/clinics/day?date=2026-09-12&clinicId=1&specializationId=2
    [HttpGet("day")]
    public async Task<IActionResult> GetDay(
        [FromQuery] DateOnly date,
        [FromQuery] int? clinicId,
        [FromQuery] int? specializationId)
    {
        var result = await _mediator.Send(new GetClinicDayQuery(date, clinicId, specializationId));
        return Ok(result.Value);
    }
}