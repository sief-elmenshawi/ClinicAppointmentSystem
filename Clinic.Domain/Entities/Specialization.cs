using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class Specialization : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Cardiology, Dermatology...
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}