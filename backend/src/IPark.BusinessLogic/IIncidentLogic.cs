using Domain;

namespace IParkBusinessLogic;
public interface IIncidentLogic
{
    public Incident GetByAttractionIdOrThrow(Guid attractionId);
    public bool ValidateIncident(Guid attractionId);
    public void CreateIncident(string description, DateTime reportedAt, Guid attractionId);
    public void ResolveIncident(Guid incidentId);

    public bool HasActiveIncidents(Guid attractionId);
    public List<Incident> GetAllIncident();
}
