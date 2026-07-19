using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Doctors.Queries.GetAvailableSlots;

public record GetAvailableSlotsQuery(int DoctorId, DateOnly Date) : IRequest<Result<List<AvailableSlotDto>>>;