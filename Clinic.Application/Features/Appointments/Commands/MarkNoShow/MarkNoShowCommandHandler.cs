using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Appointments.Commands.MarkNoShow;

public class MarkNoShowCommandHandler : IRequestHandler<MarkNoShowCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MarkNoShowCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(
        MarkNoShowCommand request, CancellationToken cancellationToken)
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
            return Result<bool>.Failure("You can only update your own appointments.", ErrorType.Forbidden);

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled
            or AppointmentStatus.NoShow)
            return Result<bool>.Failure("Cannot mark this appointment as no-show.", ErrorType.Conflict);

        appointment.Status = AppointmentStatus.NoShow;
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<bool>.Failure("This appointment was modified by another request. Please refresh and try again.", ErrorType.Conflict);
        }

        return Result<bool>.Success(true);
    }
}