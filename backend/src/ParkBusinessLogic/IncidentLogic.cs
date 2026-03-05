using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class IncidentLogic : IIncidentLogic
{
    private readonly IRepository<Incident> _repo;
    private readonly IAttractionHelperLogic _attractionLogic;

    public IncidentLogic(IRepository<Incident> repo, IAttractionHelperLogic attractionLogic)
    {
        _repo = repo;
        _attractionLogic = attractionLogic;
    }

    public Incident GetByAttractionIdOrThrow(Guid attractionId)
    {
        return _repo.Find(i => i.AttractionId == attractionId)
               ?? throw new KeyNotFoundException($"Attraction with ID {attractionId} not found.");
    }

    public bool ValidateIncident(Guid attractionId)
    {
        Incident incident = GetByAttractionIdOrThrow(attractionId);
        return incident.Resolved;
    }

    public void CreateIncident(string description, DateTime reportedAt, Guid attractionId)
    {
        Attraction attraction = _attractionLogic.GetOrThrow(attractionId);
        _repo.Add(new Incident(description, reportedAt, attraction, attractionId));
    }

    public void ResolveIncident(Guid incidentId)
    {
        var incident = _repo.Find(i => i.Id == incidentId)
                       ?? throw new KeyNotFoundException($"Incident {incidentId} not found.");
        incident.Resolved = true;
        _repo.Update(incident);
    }

    public bool HasActiveIncidents(Guid attractionId) =>
        (_repo.FindAll(i => i.AttractionId == attractionId && !i.Resolved) ?? Enumerable.Empty<Incident>()).Any();

    public List<Incident> GetAllIncident()
    {
        return _repo.FindAll().ToList();
    }
}