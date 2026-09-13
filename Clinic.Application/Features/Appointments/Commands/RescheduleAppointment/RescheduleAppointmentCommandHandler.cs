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
            return Result<bool>.Failure("Appointment not found.", ErrorType.NotFound);

        var isOwner = appointment.Patient.ApplicationUserId == _currentUserService.UserId;
        if (!isOwner)
            return Result<bool>.Failure("You are not authorized to reschedule this appointment.", ErrorType.Forbidden);

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            return Result<bool>.Failure("Cannot reschedule this appointment.", ErrorType.Conflict);

        // تحقق إن الميعاد الجديد مش محجوز أصلاً
        var slotTaken = await _context.Appointments
            .AnyAsync(a => a.DoctorId == appointment.DoctorId
                        && a.AppointmentDateTime == request.NewDateTime
                        && a.Status != AppointmentStatus.Cancelled
                        && a.Id != appointment.Id,
                cancellationToken);

        if (slotTaken)
            return Result<bool>.Failure("The new slot is already booked.", ErrorType.Conflict);

        // تحقق إن الميعاد الجديد ضمن ساعات شغل الدكتور ومرصوف بالـ Slots (نفس منطق الـ Create)
        var newDate = DateOnly.FromDateTime(request.NewDateTime);
        var isUnavailable = await _context.DoctorUnavailabilities
            .AnyAsync(u => u.DoctorId == appointment.DoctorId && u.Date == newDate, cancellationToken);

        if (isUnavailable)
            return Result<bool>.Failure("The doctor is unavailable on this date.", ErrorType.Conflict);

        var workingHour = await _context.DoctorWorkingHours
            .Where(w => w.DoctorId == appointment.DoctorId && w.DayOfWeek == request.NewDateTime.DayOfWeek)
            .FirstOrDefaultAsync(w => request.NewDateTime.TimeOfDay >= w.StartTime
                                      && request.NewDateTime.TimeOfDay < w.EndTime,
                cancellationToken);

        if (workingHour is null)
            return Result<bool>.Failure("This time is outside the doctor's working hours.");

        var minutesFromStart = (request.NewDateTime.TimeOfDay - workingHour.StartTime).TotalMinutes;
        if (minutesFromStart % workingHour.SlotDurationMinutes != 0)
            return Result<bool>.Failure("Invalid appointment time slot.");

        appointment.AppointmentDateTime = request.NewDateTime;
        appointment.Status = AppointmentStatus.Pending; // يرجع يحتاج تأكيد تاني

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
        {
            return Result<bool>.Failure("The new slot was just booked by someone else.", ErrorType.Conflict);
        }

        return Result<bool>.Success(true);
    }
}