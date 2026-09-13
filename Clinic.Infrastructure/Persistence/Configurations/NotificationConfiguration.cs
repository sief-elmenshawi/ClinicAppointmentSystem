using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Type)
            .HasMaxLength(30);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(n => n.Doctor)
            .WithMany()
            .HasForeignKey(n => n.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.DoctorId, n.IsRead });
        builder.HasIndex(n => new { n.DoctorId, n.CreatedAt });
    }
}