using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Appointments.Commands.CompleteAppointment;

public record CompleteAppointmentCommand : IRequest<Result<bool>>
{
    public int AppointmentId { get; init; }
    public string Notes { get; init; } = string.Empty;
}