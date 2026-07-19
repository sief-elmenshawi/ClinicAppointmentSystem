using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class DoctorWorkingHour : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; } // Monday, Tuesday...
    public TimeSpan StartTime { get; set; }  // 10:00
    public TimeSpan EndTime { get; set; }    // 14:00
    public int SlotDurationMinutes { get; set; } = 30; // كل حجز 30 دقيقة
}