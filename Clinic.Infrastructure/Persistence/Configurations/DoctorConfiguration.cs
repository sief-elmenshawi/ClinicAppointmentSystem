using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.Property(d => d.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.ConsultationFee)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(d => d.Specialization)
            .WithMany(s => s.Doctors)
            .HasForeignKey(d => d.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.WorkingHours)
            .WithOne(w => w.Doctor)
            .HasForeignKey(w => w.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}