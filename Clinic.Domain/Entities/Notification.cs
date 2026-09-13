using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class Notification : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public int? AppointmentId { get; set; }

    // NewBooking, BookingConfirmed, BookingCancelled, Reminder
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}