using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class Patient : BaseEntity , ISoftDelete
{
    public string FullName { get; set; } = string.Empty;
    public string ApplicationUserId { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? MedicalHistory { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}