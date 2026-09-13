using FluentValidation;

namespace Clinic.Application.Features.Clinics.Commands.CreateClinic;

public class CreateClinicCommandValidator : AbstractValidator<CreateClinicCommand>
{
    public CreateClinicCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Clinic name is required.")
            .MaximumLength(100);
    }
}