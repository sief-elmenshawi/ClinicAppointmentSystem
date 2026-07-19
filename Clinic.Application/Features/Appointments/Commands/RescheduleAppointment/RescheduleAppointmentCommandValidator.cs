using FluentValidation;

namespace Clinic.Application.Features.Appointments.Commands.RescheduleAppointment;

public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).GreaterThan(0);
        RuleFor(x => x.NewDateTime).GreaterThan(DateTime.UtcNow);
    }
}