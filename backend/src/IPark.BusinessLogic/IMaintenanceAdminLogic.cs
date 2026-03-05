using Domain;

namespace IParkBusinessLogic;

public interface IMaintenanceAdminLogic
{
    Task<Guid> ScheduleAsync(Guid attractionId, DateTime startAt, int durationMinutes, string description, CancellationToken ct = default);

    Task CancelAsync(Guid maintenanceId, DateTime when, CancellationToken ct = default);
}
