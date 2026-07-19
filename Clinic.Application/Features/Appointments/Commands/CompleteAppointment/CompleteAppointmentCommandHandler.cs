using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Appointments.Commands.CompleteAppointment;

public class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CompleteAppointmentCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null)
            return Result<bool>.Failure("Appointment not found.");

        // تحقق إن الدكتور صاحب التوكن هو نفسه صاحب الحجز
        if (appointment.Doctor.ApplicationUserId != _currentUserService.UserId)
            return Result<bool>.Failure("You are not authorized to complete this appointment.");

        if (appointment.Status == AppointmentStatus.Cancelled)
            return Result<bool>.Failure("Cannot complete a cancelled appointment.");

        if (appointment.Status == AppointmentStatus.Completed)
            return Result<bool>.Failure("Appointment is already completed.");

        appointment.Status = AppointmentStatus.Completed;
        appointment.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}