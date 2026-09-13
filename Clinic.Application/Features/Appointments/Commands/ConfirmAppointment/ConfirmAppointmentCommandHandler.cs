using Clinic.Application.Common;
using Clinic.Application.Features.Appointments.Events;
using Clinic.Application.Interfaces;
using Clinic.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Appointments.Commands.ConfirmAppointment;

public class ConfirmAppointmentCommandHandler
    : IRequestHandler<ConfirmAppointmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly IPublisher _publisher;

    public ConfirmAppointmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        IPublisher publisher)
    {
        _context = context;
        _currentUserService = currentUserService;
        _identityService = identityService;
        _publisher = publisher;
    }

    public async Task<Result<bool>> Handle(
        ConfirmAppointmentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            return Result<bool>.Failure("Unauthorized.", ErrorType.Forbidden);

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.ApplicationUserId == currentUserId, cancellationToken);

        if (doctor is null)
            return Result<bool>.Failure("Doctor profile not found.", ErrorType.NotFound);

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null)
            return Result<bool>.Failure("Appointment not found.", ErrorType.NotFound);

        if (appointment.DoctorId != doctor.Id)
            return Result<bool>.Failure("You can only confirm your own appointments.", ErrorType.Forbidden);

        if (appointment.Status != AppointmentStatus.Pending)
            return Result<bool>.Failure("Only pending appointments can be confirmed.", ErrorType.Conflict);

        appointment.Status = AppointmentStatus.Confirmed;
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<bool>.Failure("This appointment was modified by another request. Please refresh and try again.", ErrorType.Conflict);
        }

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == appointment.PatientId, cancellationToken);

        if (patient is not null)
        {
            var email = await _identityService.GetUserEmailAsync(patient.ApplicationUserId);
            if (email is not null)
            {
                await _publisher.Publish(
                    new AppointmentConfirmedEvent(
                        appointment.Id,
                        doctor.FullName,
                        patient.FullName,
                        email,
                        appointment.AppointmentDateTime),
                    cancellationToken);
            }
        }

        return Result<bool>.Success(true);
    }
}