using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Enums;
using static Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions;
using MediatR;

namespace Clinic.Application.Features.Doctors.Queries.GetAvailableSlots;

public class GetAvailableSlotsQueryHandler
    : IRequestHandler<GetAvailableSlotsQuery, Result<List<AvailableSlotDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableSlotsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AvailableSlotDto>>> Handle(
        GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {

        var isUnavailable = await _context.DoctorUnavailabilities
            .AnyAsync(u => u.DoctorId == request.DoctorId && u.Date == request.Date, cancellationToken);

        if (isUnavailable)
            return Result<List<AvailableSlotDto>>.Success(new List<AvailableSlotDto>());

        var dayOfWeek = request.Date.DayOfWeek;

        // 1. هات مواعيد شغل الدكتور في اليوم ده
        var workingHours = await _context.DoctorWorkingHours
            .Where(w => w.DoctorId == request.DoctorId && w.DayOfWeek == dayOfWeek)
            .ToListAsync(cancellationToken);

        if (!workingHours.Any())
            return Result<List<AvailableSlotDto>>.Success(new List<AvailableSlotDto>());

        // 2. هات الحجوزات الموجودة بالفعل في اليوم ده (Pending أو Confirmed بس)
        var dayStart = request.Date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        var bookedSlots = await _context.Appointments
            .Where(a => a.DoctorId == request.DoctorId
                        && a.AppointmentDateTime >= dayStart
                        && a.AppointmentDateTime < dayEnd
                        && a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.AppointmentDateTime)
            .ToListAsync(cancellationToken);

        var bookedSet = bookedSlots.ToHashSet();

        // 3. ولّد كل الـ Slots الممكنة من مواعيد الشغل، واستبعد المحجوز
        var availableSlots = new List<AvailableSlotDto>();

        foreach (var wh in workingHours)
        {
            var slotStart = request.Date.ToDateTime(TimeOnly.FromTimeSpan(wh.StartTime));
            var shiftEnd = request.Date.ToDateTime(TimeOnly.FromTimeSpan(wh.EndTime));

            while (slotStart.AddMinutes(wh.SlotDurationMinutes) <= shiftEnd)
            {
                if (!bookedSet.Contains(slotStart))
                {
                    var slotEnd = slotStart.AddMinutes(wh.SlotDurationMinutes);
                    availableSlots.Add(new AvailableSlotDto(slotStart, slotEnd));
                }

                slotStart = slotStart.AddMinutes(wh.SlotDurationMinutes);
            }
        }

        return Result<List<AvailableSlotDto>>.Success(availableSlots);
    }
}