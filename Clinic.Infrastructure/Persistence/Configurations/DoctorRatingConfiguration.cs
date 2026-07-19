using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class DoctorRatingConfiguration : IEntityTypeConfiguration<DoctorRating>
{
    public void Configure(EntityTypeBuilder<DoctorRating> builder)
    {
        builder.Property(r => r.Comment).HasMaxLength(500);

        builder.HasOne(r => r.Appointment)
            .WithOne()
            .HasForeignKey<DoctorRating>(r => r.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Doctor)
            .WithMany()
            .HasForeignKey(r => r.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Patient)
            .WithMany()
            .HasForeignKey(r => r.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // مريض واحد يقيّم كل Appointment مرة واحدة بس
        builder.HasIndex(r => r.AppointmentId).IsUnique();
    }
}