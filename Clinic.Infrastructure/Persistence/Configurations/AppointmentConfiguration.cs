using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.Property(a => a.Notes)
            .HasMaxLength(500);

        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔒 دي أهم سطر في كل الـ Configuration
        // بيمنع حجز نفس الدكتور في نفس الميعاد بالظبط على مستوى الـ Database
        builder.HasIndex(a => new { a.DoctorId, a.AppointmentDateTime })
            .IsUnique()
            .HasFilter("[Status] <> 4"); // 4 = Cancelled، يعني الملغي مش بيتحسب في المنع

        builder.Property(a => a.RowVersion)
            .IsRowVersion();
    }
}