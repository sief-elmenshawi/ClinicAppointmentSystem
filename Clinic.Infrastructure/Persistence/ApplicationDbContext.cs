using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using Clinic.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<DoctorWorkingHour> DoctorWorkingHours => Set<DoctorWorkingHour>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<DoctorRating> DoctorRatings => Set<DoctorRating>();
    public DbSet<DoctorUnavailability> DoctorUnavailabilities => Set<DoctorUnavailability>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.Entity<Doctor>().HasQueryFilter(d => !d.IsDeleted);
        builder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);

        builder.Entity<Appointment>().HasQueryFilter(a => !a.Doctor.IsDeleted);
        builder.Entity<DoctorWorkingHour>().HasQueryFilter(w => !w.Doctor.IsDeleted);
        builder.Entity<DoctorRating>().HasQueryFilter(r => !r.Doctor.IsDeleted);
        builder.Entity<DoctorUnavailability>().HasQueryFilter(u => !u.Doctor.IsDeleted);
    }
}