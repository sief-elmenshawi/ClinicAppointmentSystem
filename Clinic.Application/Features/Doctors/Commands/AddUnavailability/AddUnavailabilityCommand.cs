using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Doctors.Commands.AddUnavailability;

public record AddUnavailabilityCommand : IRequest<Result<int>>
{
    public int DoctorId { get; init; }
    public DateOnly Date { get; init; }
    public string? Reason { get; init; }
}