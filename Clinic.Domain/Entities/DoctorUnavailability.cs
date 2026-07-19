using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class DoctorUnavailability : BaseEntity
{
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DateOnly Date { get; set; }
    public string? Reason { get; set; }
}