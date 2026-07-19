using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string RequestName { get; set; } = string.Empty;
    public string? RequestData { get; set; }
    public bool IsSuccessful { get; set; }
}