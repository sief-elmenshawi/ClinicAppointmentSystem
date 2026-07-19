using FluentValidation;

namespace Clinic.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.AppointmentDateTime)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Appointment must be in the future.");
    }
}