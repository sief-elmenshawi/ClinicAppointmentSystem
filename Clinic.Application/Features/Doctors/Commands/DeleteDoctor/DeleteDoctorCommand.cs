using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Doctors.Commands.DeleteDoctor;

public record DeleteDoctorCommand(int DoctorId) : IRequest<Result<bool>>;