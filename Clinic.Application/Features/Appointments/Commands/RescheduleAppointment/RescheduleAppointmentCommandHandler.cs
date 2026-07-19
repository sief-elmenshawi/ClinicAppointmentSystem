using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Appointments.Commands.RescheduleAppointment;

public class RescheduleAppointmentCommandHandler
    : IRequestHandler<RescheduleAppointmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RescheduleAppointmentCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null)
            return Result<bool>.Failure("Appointment not found.");

        var isOwner = appointment.Patient.ApplicationUserId == _currentUserService.UserId;
        if (!isOwner)
            return Result<bool>.Failure("You are not authorized to reschedule this appointment.");

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            return Result<bool>.Failure("Cannot reschedule this appointment.");

        // تحقق إن الميعاد الجديد مش محجوز أصلاً
        var slotTaken = await _context.Appointments
            .AnyAsync(a => a.DoctorId == appointment.DoctorId
                        && a.AppointmentDateTime == request.NewDateTime
                        && a.Status != AppointmentStatus.Cancelled
                        && a.Id != appointment.Id,
                cancellationToken);

        if (slotTaken)
            return Result<bool>.Failure("The new slot is already booked.");

        appointment.AppointmentDateTime = request.NewDateTime;
        appointment.Status = AppointmentStatus.Pending; // يرجع يحتاج تأكيد تاني

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<bool>.Failure("The new slot was just booked by someone else.");
        }

        return Result<bool>.Success(true);
    }
}