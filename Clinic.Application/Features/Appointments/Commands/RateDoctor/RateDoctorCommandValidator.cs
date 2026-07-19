using FluentValidation;

namespace Clinic.Application.Features.Appointments.Commands.RateDoctor;

public class RateDoctorCommandValidator : AbstractValidator<RateDoctorCommand>
{
    public RateDoctorCommandValidator()
    {
        RuleFor(x => x.AppointmentId).GreaterThan(0);
        RuleFor(x => x.Score).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}