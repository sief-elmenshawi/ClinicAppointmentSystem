using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Appointments.Commands.ConfirmAppointment;

public record ConfirmAppointmentCommand(int AppointmentId) : IRequest<Result<bool>>;