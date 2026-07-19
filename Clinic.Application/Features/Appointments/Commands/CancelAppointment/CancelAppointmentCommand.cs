using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand(int AppointmentId) : IRequest<Result<bool>>;