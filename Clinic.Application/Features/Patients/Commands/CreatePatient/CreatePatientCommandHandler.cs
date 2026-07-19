using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using MediatR;

namespace Clinic.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CreatePatientCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result<int>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, userId, error) = await _identityService.CreateUserAsync(
            request.Email, request.Password, "Patient");

        if (!succeeded)
            return Result<int>.Failure(error ?? "Failed to create user account.");

        var patient = new Patient
        {
            FullName = request.FullName,
            ApplicationUserId = userId!,
            DateOfBirth = request.DateOfBirth,
            MedicalHistory = request.MedicalHistory
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(patient.Id);
    }
}