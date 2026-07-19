using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CancelAppointmentCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null)
            return Result<bool>.Failure("Appointment not found.");

        var currentUserId = _currentUserService.UserId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var isOwner = appointment.Doctor.ApplicationUserId == currentUserId
                      || appointment.Patient.ApplicationUserId == currentUserId;

        if (!isAdmin && !isOwner)
            return Result<bool>.Failure("You are not authorized to cancel this appointment.");

        if (appointment.Status == AppointmentStatus.Completed)
            return Result<bool>.Failure("Cannot cancel a completed appointment.");

        if (appointment.Status == AppointmentStatus.Cancelled)
            return Result<bool>.Failure("Appointment is already cancelled.");

        appointment.Status = AppointmentStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}