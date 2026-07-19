using Clinic.Application.Common;
using Clinic.Application.Features.Appointments.Events;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using Clinic.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IPublisher _publisher;

    public CreateAppointmentCommandHandler(
        IApplicationDbContext context, IIdentityService identityService, IPublisher publisher)
    {
        _context = context;
        _identityService = identityService;
        _publisher = publisher;
    }

    public async Task<Result<int>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        // 1. تأكد إن الدكتور والمريض موجودين
        var doctorExists = await _context.Doctors
            .AnyAsync(d => d.Id == request.DoctorId, cancellationToken);
        if (!doctorExists)
            return Result<int>.Failure("Doctor not found.");

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);
        if (patient is null)
            return Result<int>.Failure("Patient not found.");

        var appointmentDate = DateOnly.FromDateTime(request.AppointmentDateTime);
        var isUnavailable = await _context.DoctorUnavailabilities
            .AnyAsync(u => u.DoctorId == request.DoctorId && u.Date == appointmentDate, cancellationToken);

        if (isUnavailable)
            return Result<int>.Failure("The doctor is unavailable on this date.");

        // 2. تأكد إن الميعاد ضمن ساعات شغل الدكتور
        var dayOfWeek = request.AppointmentDateTime.DayOfWeek;
        var timeOfDay = request.AppointmentDateTime.TimeOfDay;

        var workingHour = await _context.DoctorWorkingHours
            .Where(w => w.DoctorId == request.DoctorId && w.DayOfWeek == dayOfWeek)
            .FirstOrDefaultAsync(w => timeOfDay >= w.StartTime && timeOfDay < w.EndTime, cancellationToken);

        if (workingHour is null)
            return Result<int>.Failure("This time is outside the doctor's working hours.");

        var minutesFromStart = (timeOfDay - workingHour.StartTime).TotalMinutes;
        if (minutesFromStart % workingHour.SlotDurationMinutes != 0)
            return Result<int>.Failure("Invalid appointment time slot.");

        // 3. Application-level check
        var alreadyBooked = await _context.Appointments
            .AnyAsync(a => a.DoctorId == request.DoctorId
                        && a.AppointmentDateTime == request.AppointmentDateTime
                        && a.Status != AppointmentStatus.Cancelled,
                cancellationToken);

        if (alreadyBooked)
            return Result<int>.Failure("This slot is already booked.");

        // 4. الحجز الفعلي
        var appointment = new Appointment
        {
            DoctorId = request.DoctorId,
            PatientId = request.PatientId,
            AppointmentDateTime = request.AppointmentDateTime,
            Status = AppointmentStatus.Pending
        };

        _context.Appointments.Add(appointment);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<int>.Failure("This slot was just booked by someone else. Please choose another slot.");
        }

        // 5. أطلق الـ Event للإيميل
        var email = await _identityService.GetUserEmailAsync(patient.ApplicationUserId);


        if (email is not null)
        {
            await _publisher.Publish(
                new AppointmentCreatedEvent(appointment.Id, email, appointment.AppointmentDateTime),
                cancellationToken);

        }

        return Result<int>.Success(appointment.Id);
    }
}