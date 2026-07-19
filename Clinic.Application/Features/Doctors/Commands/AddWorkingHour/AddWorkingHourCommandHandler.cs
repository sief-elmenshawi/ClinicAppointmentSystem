using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using static Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions;
using MediatR;

namespace Clinic.Application.Features.Doctors.Commands.AddWorkingHour;

public class AddWorkingHourCommandHandler : IRequestHandler<AddWorkingHourCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public AddWorkingHourCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(AddWorkingHourCommand request, CancellationToken cancellationToken)
    {
        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == request.DoctorId, cancellationToken);

        if (!doctorExists)
            return Result<int>.Failure("Doctor not found.");

        // امنع تداخل مواعيد الشغل لنفس اليوم (مثلاً 10-2 و 1-3 متعارضين)
        var overlaps = await _context.DoctorWorkingHours
            .Where(w => w.DoctorId == request.DoctorId && w.DayOfWeek == request.DayOfWeek)
            .AnyAsync(w =>
                request.StartTime < w.EndTime && request.EndTime > w.StartTime,
                cancellationToken);

        if (overlaps)
            return Result<int>.Failure("This time range overlaps with an existing working hour.");

        var workingHour = new DoctorWorkingHour
        {
            DoctorId = request.DoctorId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SlotDurationMinutes = request.SlotDurationMinutes
        };

        _context.DoctorWorkingHours.Add(workingHour);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(workingHour.Id);
    }
}