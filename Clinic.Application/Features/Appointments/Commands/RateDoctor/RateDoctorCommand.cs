using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Appointments.Commands.RateDoctor;

public record RateDoctorCommand : IRequest<Result<int>>
{
    public int AppointmentId { get; init; }
    public int Score { get; init; }
    public string? Comment { get; init; }
}