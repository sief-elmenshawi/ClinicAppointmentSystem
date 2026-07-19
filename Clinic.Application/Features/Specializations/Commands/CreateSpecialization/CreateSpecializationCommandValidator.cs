using FluentValidation;

namespace Clinic.Application.Features.Specializations.Commands.CreateSpecialization;

public class CreateSpecializationCommandValidator : AbstractValidator<CreateSpecializationCommand>
{
    public CreateSpecializationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Specialization name is required.")
            .MaximumLength(100);
    }
}