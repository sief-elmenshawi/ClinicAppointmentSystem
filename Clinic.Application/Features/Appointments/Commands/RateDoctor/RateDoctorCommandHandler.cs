using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Appointments.Commands.RateDoctor;

public class RateDoctorCommandHandler : IRequestHandler<RateDoctorCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RateDoctorCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<int>> Handle(RateDoctorCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null)
            return Result<int>.Failure("Appointment not found.");

        // تأكد إن المريض صاحب التوكن هو نفسه صاحب الحجز
        if (appointment.Patient.ApplicationUserId != _currentUserService.UserId)
            return Result<int>.Failure("You are not authorized to rate this appointment.");

        // تأكد إن الكشف اتعمل فعلاً
        if (appointment.Status != AppointmentStatus.Completed)
            return Result<int>.Failure("You can only rate completed appointments.");

        // تأكد إنه ماقيّمهاش قبل كده
        var alreadyRated = await _context.DoctorRatings
            .AnyAsync(r => r.AppointmentId == request.AppointmentId, cancellationToken);

        if (alreadyRated)
            return Result<int>.Failure("You have already rated this appointment.");

        var rating = new DoctorRating
        {
            AppointmentId = appointment.Id,
            DoctorId = appointment.DoctorId,
            PatientId = appointment.PatientId,
            Score = request.Score,
            Comment = request.Comment
        };

        _context.DoctorRatings.Add(rating);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<int>.Failure("This appointment has already been rated.");
        }

        return Result<int>.Success(rating.Id);
    }
}