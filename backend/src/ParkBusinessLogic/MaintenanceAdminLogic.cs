using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class MaintenanceAdminLogic : IMaintenanceAdminLogic
{
    private readonly IAttractionLogic _attractions;
    private readonly IIncidentLogic _incidents;
    private readonly IRepository<Maintenance> _repo;

    public MaintenanceAdminLogic(
        IAttractionLogic attractions,
        IIncidentLogic incidents,
        IRepository<Maintenance> repo)
    {
        _attractions = attractions;
        _incidents = incidents;
        _repo = repo;
    }

    public Guid Schedule(Guid attractionId, DateTime startAtUtc, int durationMinutes, string description)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(durationMinutes);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        var attraction = _attractions.GetOrThrow(attractionId);

        var maintenance = new Maintenance(
            attractionId: attraction.Id,
            attraction: attraction,
            startAt: startAtUtc,
            durationMinutes: durationMinutes,
            description: description);

        _repo.Add(maintenance);

        _incidents.CreateIncident(
    $"Maintenance window: {description}",
             startAtUtc,
             attraction.Id);

        return maintenance.Id;
    }

    public void Cancel(Guid maintenanceId, DateTime whenUtc)
    {
        var m = _repo.Find(x => x.Id == maintenanceId)
                ?? throw new KeyNotFoundException($"Maintenance {maintenanceId} not found.");
        m.Cancel(whenUtc);
        _repo.Update(m);

        if (_incidents.HasActiveIncidents(m.AttractionId))
        {
                 var inc = _incidents.GetByAttractionIdOrThrow(m.AttractionId);
                 _incidents.ResolveIncident(inc.Id);
        }
    }

    public Task<Guid> ScheduleAsync(Guid attractionId, DateTime startAtUtc, int durationMinutes, string description, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<Guid>(cancellationToken);
        }

        var id = Schedule(attractionId, startAtUtc, durationMinutes, description);
        return Task.FromResult(id);
    }

    public Task CancelAsync(Guid maintenanceId, DateTime whenUtc, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        Cancel(maintenanceId, whenUtc);
        return Task.CompletedTask;
    }
}
