using Clinic.Application.Features.Appointments.Commands.CancelAppointment;
using Clinic.Application.Features.Appointments.Commands.CompleteAppointment;
using Clinic.Application.Features.Appointments.Commands.CreateAppointment;
using Clinic.Application.Features.Appointments.Commands.RateDoctor;
using Clinic.Application.Features.Appointments.Commands.RescheduleAppointment;
using Clinic.Application.Features.Appointments.Queries.GetDoctorAppointments;
using Clinic.Application.Features.Appointments.Queries.GetPatientAppointments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppointmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAppointmentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> Complete(int id, CompleteAppointmentCommand command)
    {
        if (id != command.AppointmentId)
            return BadRequest("Appointment ID mismatch.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpDelete("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _mediator.Send(new CancelAppointmentCommand(id));
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPut("{id}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, RescheduleAppointmentCommand command)
    {
        if (id != command.AppointmentId)
            return BadRequest("Appointment ID mismatch.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetDoctorAppointments(
     int doctorId, [FromQuery] DateOnly? date,
     [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetDoctorAppointmentsQuery(doctorId, date, pageNumber, pageSize));
        return Ok(result.Value);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetPatientAppointments(int patientId)
    {
        var result = await _mediator.Send(new GetPatientAppointmentsQuery(patientId));
        return Ok(result.Value);
    }
    [HttpPost("{id}/rate")]
    public async Task<IActionResult> Rate(int id, RateDoctorCommand command)
    {
        if (id != command.AppointmentId)
            return BadRequest("Appointment ID mismatch.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}