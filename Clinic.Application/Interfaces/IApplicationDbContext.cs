using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Clinic.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Department> Departments { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Specialization> Specializations { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<DoctorWorkingHour> DoctorWorkingHours { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<AuditLog> AuditLogs { get; }        
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<DoctorRating> DoctorRatings { get; }
    DbSet<DoctorUnavailability> DoctorUnavailabilities { get; }
    ChangeTracker ChangeTracker { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}