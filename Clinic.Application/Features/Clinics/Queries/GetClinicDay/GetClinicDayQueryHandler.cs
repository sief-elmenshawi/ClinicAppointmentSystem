using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Clinics.Queries.GetClinicDay;

public class GetClinicDayQueryHandler : IRequestHandler<GetClinicDayQuery, Result<List<ClinicDayDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly DoctorDayService _doctorDayService;

    public GetClinicDayQueryHandler(
        IApplicationDbContext context, DoctorDayService doctorDayService)
    {
        _context = context;
        _doctorDayService = doctorDayService;
    }

    public async Task<Result<List<ClinicDayDto>>> Handle(
        GetClinicDayQuery request, CancellationToken cancellationToken)
    {
        var departments = await _context.Departments
            .AsNoTracking()
            .Include(d => d.Specializations)
            .ThenInclude(s => s.Doctors)
            .Where(d => request.ClinicId == null || d.Id == request.ClinicId)
            .Where(d => request.SpecializationId == null
                        || d.Specializations.Any(s => s.Id == request.SpecializationId))
            .OrderBy(d => d.Id)
            .ToListAsync(cancellationToken);

        if (departments.Count == 0)
            return Result<List<ClinicDayDto>>.Success(new List<ClinicDayDto>());

        // حدد الأطباء لكل قسم مع تطبيق فلتر التخصص بسرعة في الذاكرة
        var doctorsByDepartment = new List<(Department Department, List<Doctor> Doctors)>();
        var allDoctors = new List<Doctor>();

        foreach (var department in departments)
        {
            var doctors = department.Specializations
                .SelectMany(s => s.Doctors)
                .Where(d => request.SpecializationId == null || d.SpecializationId == request.SpecializationId)
                .Distinct()
                .ToList();

            if (doctors.Count == 0)
                continue;

            doctorsByDepartment.Add((department, doctors));
            allDoctors.AddRange(doctors);
        }

        // احسب حالة كل الأطباء في الـ Day في جولة واحدة بدل جولة لكل قسم
        var statuses = await _doctorDayService.ComputeAsync(allDoctors, request.Date, cancellationToken);
        var statusByDoctorId = statuses.ToDictionary(s => s.DoctorId);

        var result = doctorsByDepartment
            .Select(group => new ClinicDayDto(
                group.Department.Id,
                group.Department.Name,
                group.Doctors
                    .Select(d => statusByDoctorId.TryGetValue(d.Id, out var status) ? status : null)
                    .Where(s => s is not null)
                    .ToList()!))
            .ToList();

        return Result<List<ClinicDayDto>>.Success(result);
    }
}