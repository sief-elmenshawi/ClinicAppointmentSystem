using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class Specialization : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Cardiology, Dermatology...

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}