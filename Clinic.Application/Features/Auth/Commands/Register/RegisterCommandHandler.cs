using Clinic.Application.Common;
using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using MediatR;

namespace Clinic.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<Result<int>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 👇 الـ Role هنا Hardcoded على "Patient" - المستخدم مش بيقدر يتحكم فيها خالص
        var (succeeded, userId, error) = await _identityService.CreateUserAsync(
            request.Email, request.Password, "Patient");

        if (!succeeded)
            return Result<int>.Failure(error ?? "Registration failed.");

        var patient = new Patient
        {
            FullName = request.FullName,
            ApplicationUserId = userId!,
            DateOfBirth = request.DateOfBirth
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(patient.Id);
    }
}