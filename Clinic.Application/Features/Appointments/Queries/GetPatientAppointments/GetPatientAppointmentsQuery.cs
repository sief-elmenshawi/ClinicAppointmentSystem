using Clinic.Application.Common;
using MediatR;

namespace Clinic.Application.Features.Appointments.Queries.GetPatientAppointments;

public record GetPatientAppointmentsQuery(int PatientId) : IRequest<Result<List<AppointmentDto>>>;