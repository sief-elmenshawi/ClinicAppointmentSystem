using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Tests;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : base(options) { }

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<DoctorWorkingHour> DoctorWorkingHours => Set<DoctorWorkingHour>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<DoctorRating> DoctorRatings => Set<DoctorRating>();

    public DbSet<DoctorUnavailability> DoctorUnavailabilities => Set<DoctorUnavailability>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ضروري نعمل Override للـ Unique Index هنا كمان عشان الـ InMemory Provider يحاكي نفس سلوك الـ Double Booking
        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.DoctorId, a.AppointmentDateTime })
            .IsUnique();
    }
}