using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class Doctor : BaseEntity , ISoftDelete
{
    public string FullName { get; set; } = string.Empty;
    public string ApplicationUserId { get; set; } = string.Empty; // Link لـ Identity
    public int SpecializationId { get; set; }
    public Specialization Specialization { get; set; } = null!;
    public decimal ConsultationFee { get; set; }

    public ICollection<DoctorWorkingHour> WorkingHours { get; set; } = new List<DoctorWorkingHour>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}