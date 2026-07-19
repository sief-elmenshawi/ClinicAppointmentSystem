using Clinic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Doctor> Doctors { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Specialization> Specializations { get; }
    DbSet<DoctorWorkingHour> DoctorWorkingHours { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<AuditLog> AuditLogs { get; }        
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<DoctorRating> DoctorRatings { get; }
    DbSet<DoctorUnavailability> DoctorUnavailabilities { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}