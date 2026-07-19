using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand : IRequest<Result<int>>
{
    public int DoctorId { get; init; }
    public int PatientId { get; init; }
    public DateTime AppointmentDateTime { get; init; }
}