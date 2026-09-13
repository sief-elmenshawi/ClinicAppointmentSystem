using Clinic.Application.Features.Doctors.Commands.AddUnavailability;
using Clinic.Application.Features.Doctors.Commands.AddWorkingHour;
using Clinic.Application.Features.Doctors.Commands.CreateDoctor;
using Clinic.Application.Features.Doctors.Commands.DeleteDoctor;
using Clinic.Application.Features.Doctors.Queries.GetAllDoctors;
using Clinic.Application.Features.Doctors.Queries.GetAvailableSlots;
using Clinic.Application.Features.Doctors.Queries.GetCurrentDoctor;
using Clinic.Application.Features.Doctors.Queries.GetDoctorRatings;
using Clinic.Application.Features.Doctors.Queries.GetDoctorsBySpecialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateDoctorCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToHttpResult();
    }

    [Authorize(Roles = "Doctor")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await _mediator.Send(new GetCurrentDoctorQuery());
        return result.ToHttpResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllDoctorsQuery(pageNumber, pageSize));
        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{doctorId}/working-hours")]
    public async Task<IActionResult> AddWorkingHour(int doctorId, AddWorkingHourCommand command)
    {
        if (doctorId != command.DoctorId)
            return BadRequest("Doctor ID mismatch.");

        var result = await _mediator.Send(command);
        return result.ToHttpResult();
    }
    [AllowAnonymous]
    [HttpGet("{doctorId}/available-slots")]
    public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateOnly date)
    {
        var result = await _mediator.Send(new GetAvailableSlotsQuery(doctorId, date));
        return Ok(result.Value);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteDoctorCommand(id));
        return result.ToHttpActionResult();
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

    [Authorize(Roles = "Admin")]
    [HttpPost("{doctorId}/unavailability")]
    public async Task<IActionResult> AddUnavailability(int doctorId, AddUnavailabilityCommand command)
    {
        if (doctorId != command.DoctorId)
            return BadRequest("Doctor ID mismatch.");

        var result = await _mediator.Send(command);
        return result.ToHttpResult();
    }
}