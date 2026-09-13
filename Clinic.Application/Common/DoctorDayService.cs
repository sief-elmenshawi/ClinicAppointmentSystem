using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Common;

public class DoctorDayService
{
    private readonly IApplicationDbContext _context;

    public DoctorDayService(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// يحسب لكل دكتور حالته في يوم معين:
    /// Absent (إجازة) - Off (مش يوم شغل) - Working (عنده حجوزات) - Free (يوم شغل من غير حجوزات)
    /// </summary>
    public async Task<List<DoctorDayStatusDto>> ComputeAsync(
        List<Doctor> doctors, DateOnly date, CancellationToken cancellationToken)
    {
        if (doctors.Count == 0)
            return new List<DoctorDayStatusDto>();

        var doctorIds = doctors.Select(d => d.Id).ToList();
        var dayOfWeek = date.DayOfWeek;

        var absentIds = (await _context.DoctorUnavailabilities
            .AsNoTracking()
            .Where(u => doctorIds.Contains(u.DoctorId) && u.Date == date)
            .Select(u => u.DoctorId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var hours = await _context.DoctorWorkingHours
            .AsNoTracking()
            .Where(w => doctorIds.Contains(w.DoctorId) && w.DayOfWeek == dayOfWeek)
            .ToListAsync(cancellationToken);

        var hoursByDoctor = hours
            .GroupBy(h => h.DoctorId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        var bookedByDoctor = await _context.Appointments
            .AsNoTracking()
            .Where(a => doctorIds.Contains(a.DoctorId)
                        && a.AppointmentDateTime >= dayStart
                        && a.AppointmentDateTime < dayEnd
                        && a.Status != AppointmentStatus.Cancelled)
            .GroupBy(a => a.DoctorId)
            .Select(g => new { DoctorId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DoctorId, x => x.Count, cancellationToken);

        var result = new List<DoctorDayStatusDto>();

        foreach (var doctor in doctors)
        {
            var specName = doctor.Specialization?.Name ?? string.Empty;

            if (absentIds.Contains(doctor.Id))
            {
                result.Add(new DoctorDayStatusDto(
                    doctor.Id, doctor.FullName, specName, doctor.ConsultationFee,
                    null, null, 0, 0, 0, "Absent"));
                continue;
            }

            var isWorkingToday = hoursByDoctor.TryGetValue(doctor.Id, out var workingHour);
            if (!isWorkingToday || workingHour!.Count == 0)
            {
                result.Add(new DoctorDayStatusDto(
                    doctor.Id, doctor.FullName, specName, doctor.ConsultationFee,
                    null, null, 0, 0, 0, "Off"));
                continue;
            }

            var start = workingHour.Min(w => w.StartTime);
            var end = workingHour.Max(w => w.EndTime);
            var durationMinutes = workingHour.First().SlotDurationMinutes;

            var totalSlots = workingHour.Sum(w =>
                (int)((w.EndTime - w.StartTime).TotalMinutes / w.SlotDurationMinutes));

            var bookedCount = bookedByDoctor.TryGetValue(doctor.Id, out var c) ? c : 0;
            var availableCount = Math.Max(0, totalSlots - bookedCount);
            var status = bookedCount > 0 ? "Working" : "Free";

            result.Add(new DoctorDayStatusDto(
                doctor.Id, doctor.FullName, specName, doctor.ConsultationFee,
                start, end, durationMinutes, bookedCount, availableCount, status));
        }

        return result;
    }
}