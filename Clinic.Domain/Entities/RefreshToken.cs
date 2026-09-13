using Clinic.Domain.Common;

namespace Clinic.Domain.Entities;

public class RefreshToken : BaseEntity
{
    // بيرخّن SHA-256 hash بتاع الـ token مش الـ token نفسه،
    // عشان لو الـ DB اتسرقت ميكونش استخدم أي token
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
}