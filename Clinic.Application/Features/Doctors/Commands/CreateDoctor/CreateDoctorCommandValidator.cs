using FluentValidation;

namespace Clinic.Application.Features.Doctors.Commands.CreateDoctor;

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.SpecializationId).GreaterThan(0);
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0);
    }
}