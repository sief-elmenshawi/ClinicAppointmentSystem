using FluentValidation;

namespace Clinic.Application.Features.Doctors.Commands.AddUnavailability;

public class AddUnavailabilityCommandValidator : AbstractValidator<AddUnavailabilityCommand>
{
    public AddUnavailabilityCommandValidator()
    {
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.Date).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.Reason).MaximumLength(300);
    }
}