using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Admin.Queries.GetAdminDashboard;

public class GetAdminDashboardQueryHandler
    : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly DoctorDayService _doctorDayService;

    public GetAdminDashboardQueryHandler(
        IApplicationDbContext context, DoctorDayService doctorDayService)
    {
        _context = context;
        _doctorDayService = doctorDayService;
    }

    public async Task<Result<AdminDashboardDto>> Handle(
        GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var date = request.Date;
        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        var todayQuery = _context.Appointments
            .AsNoTracking()
            .Where(a => a.AppointmentDateTime >= dayStart && a.AppointmentDateTime < dayEnd);

        // عد واحدة مجمّعة حسب الـ Status بدل 6 جولات DB منفصلة
        var grouped = await todayQuery
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var counts = grouped.ToDictionary(g => g.Status, g => g.Count);
        int Count(AppointmentStatus s) => counts.TryGetValue(s, out var c) ? c : 0;

        var totalAppointments = counts.Values.Sum();
        var pending = Count(AppointmentStatus.Pending);
        var confirmed = Count(AppointmentStatus.Confirmed);
        var completed = Count(AppointmentStatus.Completed);
        var cancelled = Count(AppointmentStatus.Cancelled);
        var noShow = Count(AppointmentStatus.NoShow);

        var todayRevenue = await todayQuery
            .Where(a => a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed)
            .Join(
                _context.Doctors,
                a => a.DoctorId,
                d => d.Id,
                (a, d) => d.ConsultationFee)
            .SumAsync(cancellationToken);

        var clinicsCount = await _context.Departments.CountAsync(cancellationToken);
        var doctorsCount = await _context.Doctors.CountAsync(cancellationToken);
        var patientsCount = await _context.Patients.CountAsync(cancellationToken);

        var waitingList = await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Pending && a.AppointmentDateTime >= dayStart)
            .OrderBy(a => a.AppointmentDateTime)
            .Take(20)
            .Select(a => new WaitingListItemDto(
                a.Id,
                a.Patient.FullName,
                a.Doctor.FullName,
                a.Doctor.Specialization.Department.Name,
                a.AppointmentDateTime,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        var doctors = await _context.Doctors
            .AsNoTracking()
            .Include(d => d.Specialization)
            .ToListAsync(cancellationToken);

        var doctorsToday = await _doctorDayService.ComputeAsync(doctors, date, cancellationToken);

        var working = doctorsToday.Count(s => s.Status == "Working");
        var free = doctorsToday.Count(s => s.Status == "Free");
        var absent = doctorsToday.Count(s => s.Status == "Absent");

        var overview = new DashboardOverviewDto(
            totalAppointments, pending, confirmed, completed, cancelled, noShow,
            working + free, working, free, absent,
            clinicsCount, doctorsCount, patientsCount, todayRevenue);

        return Result<AdminDashboardDto>.Success(new AdminDashboardDto(
            date.ToString("yyyy-MM-dd"), overview, waitingList, doctorsToday));
    }
}