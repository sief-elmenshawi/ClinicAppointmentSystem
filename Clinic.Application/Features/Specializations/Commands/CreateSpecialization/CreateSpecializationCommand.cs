using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Specializations.Commands.CreateSpecialization;

public record CreateSpecializationCommand(string Name) : IRequest<Result<int>>;