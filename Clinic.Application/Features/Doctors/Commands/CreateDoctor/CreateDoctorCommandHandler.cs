using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using static Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions;
using MediatR;

namespace Clinic.Application.Features.Doctors.Commands.CreateDoctor;

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CreateDoctorCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result<int>> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        // 1. تحقق إن الـ Specialization موجودة
        var specializationExists = await _context.Specializations
            .AnyAsync(s => s.Id == request.SpecializationId, cancellationToken);

        if (!specializationExists)
            return Result<int>.Failure("Specialization not found.");

        // 2. اعمل الـ Identity User الأول
        var (succeeded, userId, error) = await _identityService.CreateUserAsync(
            request.Email, request.Password, "Doctor");

        if (!succeeded)
            return Result<int>.Failure(error ?? "Failed to create user account.");

        // 3. اعمل الـ Doctor entity واربطه بالـ User
        var doctor = new Doctor
        {
            FullName = request.FullName,
            ApplicationUserId = userId!,
            SpecializationId = request.SpecializationId,
            ConsultationFee = request.ConsultationFee
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(doctor.Id);
    }
}