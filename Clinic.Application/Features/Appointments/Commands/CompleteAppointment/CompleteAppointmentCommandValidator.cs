using FluentValidation;

namespace Clinic.Application.Features.Appointments.Commands.CompleteAppointment;

public class CompleteAppointmentCommandValidator : AbstractValidator<CompleteAppointmentCommand>
{
    public CompleteAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).GreaterThan(0);
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(500);
    }
}