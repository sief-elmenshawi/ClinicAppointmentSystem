using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Appointments.Commands.RescheduleAppointment;

public record RescheduleAppointmentCommand : IRequest<Result<bool>>
{
    public int AppointmentId { get; init; }
    public DateTime NewDateTime { get; init; }
}