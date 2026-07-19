using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class DoctorRating : BaseEntity
{
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int Score { get; set; } 
    public string? Comment { get; set; }
}