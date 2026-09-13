using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();
}