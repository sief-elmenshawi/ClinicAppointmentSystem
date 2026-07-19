namespace Clinic.Application.Interfaces;

public interface IAppointmentCleanupService
{
    Task CancelStalePendingAppointmentsAsync();
}