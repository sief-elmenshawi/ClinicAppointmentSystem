using FluentValidation;

namespace Clinic.Application.Features.Doctors.Commands.AddWorkingHour;

public class AddWorkingHourCommandValidator : AbstractValidator<AddWorkingHourCommand>
{
    public AddWorkingHourCommandValidator()
    {
        RuleFor(x => x.DoctorId).GreaterThan(0);

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");

        RuleFor(x => x.SlotDurationMinutes)
            .InclusiveBetween(10, 120)
            .WithMessage("Slot duration must be between 10 and 120 minutes.");
    }
}