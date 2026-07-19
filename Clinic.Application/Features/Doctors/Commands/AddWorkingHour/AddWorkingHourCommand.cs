using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Doctors.Commands.AddWorkingHour;

public record AddWorkingHourCommand : IRequest<Result<int>>
{
    public int DoctorId { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public int SlotDurationMinutes { get; init; } = 30;
}