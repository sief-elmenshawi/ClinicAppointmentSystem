using Clinic.Application.Features.Doctors.Commands.AddUnavailability;
using Clinic.Application.Features.Doctors.Commands.AddWorkingHour;
using Clinic.Application.Features.Doctors.Commands.CreateDoctor;
using Clinic.Application.Features.Doctors.Commands.DeleteDoctor;
using Clinic.Application.Features.Doctors.Queries.GetAvailableSlots;
using Clinic.Application.Features.Doctors.Queries.GetDoctorRatings;
using Clinic.Application.Features.Doctors.Queries.GetDoctorsBySpecialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles ="Admin")]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDoctorCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("{doctorId}/working-hours")]
    public async Task<IActionResult> AddWorkingHour(int doctorId, AddWorkingHourCommand command)
    {
        if (doctorId != command.DoctorId)
            return BadRequest("Doctor ID mismatch.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [AllowAnonymous]
    [HttpGet("{doctorId}/available-slots")]
    public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateOnly date)
    {
        var result = await _mediator.Send(new GetAvailableSlotsQuery(doctorId, date));
        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteDoctorCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [AllowAnonymous]
    [HttpGet("by-specialization/{specializationId}")]
    public async Task<IActionResult> GetBySpecialization(int specializationId)
    {
        var result = await _mediator.Send(new GetDoctorsBySpecializationQuery(specializationId));
        return Ok(result.Value);
    }
    [AllowAnonymous]
    [HttpGet("{doctorId}/ratings")]
    public async Task<IActionResult> GetRatings(int doctorId)
    {
        var result = await _mediator.Send(new GetDoctorRatingsQuery(doctorId));
        return Ok(result.Value);
    }

    [HttpPost("{doctorId}/unavailability")]
    public async Task<IActionResult> AddUnavailability(int doctorId, AddUnavailabilityCommand command)
    {
        if (doctorId != command.DoctorId)
            return BadRequest("Doctor ID mismatch.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}