using Clinic.Application.Features.Appointments.Commands.CancelAppointment;
using Clinic.Application.Features.Appointments.Commands.CompleteAppointment;
using Clinic.Application.Features.Appointments.Commands.ConfirmAppointment;
using Clinic.Application.Features.Appointments.Commands.CreateAppointment;
using Clinic.Application.Features.Appointments.Commands.MarkNoShow;
using Clinic.Application.Features.Appointments.Commands.RateDoctor;
using Clinic.Application.Features.Appointments.Commands.RescheduleAppointment;
using Clinic.Application.Features.Appointments.Queries.GetAllAppointments;
using Clinic.Application.Features.Appointments.Queries.GetDoctorAppointments;
using Clinic.Application.Features.Appointments.Queries.GetPatientAppointments;
using Clinic.Domain.Enums;
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

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int? clinicId,
        [FromQuery] int? doctorId,
        [FromQuery] AppointmentStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllAppointmentsQuery(
            fromDate, toDate, clinicId, doctorId, status, pageNumber, pageSize));
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAppointmentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToHttpResult();
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> Complete(int id, CompleteAppointmentCommand command)
    {
        if (id != command.AppointmentId)
            return BadRequest("Appointment ID mismatch.");

        var result = await _mediator.Send(command);
        return result.ToHttpActionResult();
    }

    [HttpDelete("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _mediator.Send(new CancelAppointmentCommand(id));
        return result.ToHttpActionResult();
    }

    [HttpPut("{id}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, RescheduleAppointmentCommand command)
    {
        if (id != command.AppointmentId)
            return BadRequest("Appointment ID mismatch.");

        var result = await _mediator.Send(command);
        return result.ToHttpActionResult();
    }
    [Authorize(Roles = "Doctor")]
    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id)
    {
        var result = await _mediator.Send(new ConfirmAppointmentCommand(id));
        return result.ToHttpActionResult();
    }

    [Authorize(Roles = "Doctor")]
    [HttpPut("{id}/no-show")]
    public async Task<IActionResult> MarkNoShow(int id)
    {
        var result = await _mediator.Send(new MarkNoShowCommand(id));
        return result.ToHttpActionResult();
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetDoctorAppointments(
     int doctorId, [FromQuery] DateOnly? date,
     [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetDoctorAppointmentsQuery(doctorId, date, pageNumber, pageSize));
        return result.ToHttpResult();
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetPatientAppointments(int patientId)
    {
        var result = await _mediator.Send(new GetPatientAppointmentsQuery(patientId));
        return result.ToHttpResult();
    }
    [HttpPost("{id}/rate")]
    public async Task<IActionResult> Rate(int id, RateDoctorCommand command)
    {
        if (id != command.AppointmentId)
            return BadRequest("Appointment ID mismatch.");

        var result = await _mediator.Send(command);
        return result.ToHttpResult();
    }
}