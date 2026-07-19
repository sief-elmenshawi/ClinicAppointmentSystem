using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class DoctorUnavailabilityConfiguration : IEntityTypeConfiguration<DoctorUnavailability>
{
    public void Configure(EntityTypeBuilder<DoctorUnavailability> builder)
    {
        builder.Property(u => u.Reason).HasMaxLength(300);

        builder.HasOne(u => u.Doctor)
            .WithMany()
            .HasForeignKey(u => u.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // دكتور مايقدرش يقفل نفس اليوم مرتين
        builder.HasIndex(u => new { u.DoctorId, u.Date }).IsUnique();
    }
}