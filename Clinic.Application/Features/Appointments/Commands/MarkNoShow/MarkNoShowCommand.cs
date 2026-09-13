using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Appointments.Commands.MarkNoShow;

public record MarkNoShowCommand(int AppointmentId) : IRequest<Result<bool>>;