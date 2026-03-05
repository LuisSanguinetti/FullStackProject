using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class SpecialEventLogic : ISpecialEventLogic
{
    private readonly IRepository<SpecialEvent> _repo;

    public SpecialEventLogic(IRepository<SpecialEvent> repo)
    {
        _repo = repo;
    }

    public SpecialEvent GetOrThrow(Guid? id)
    {
        return _repo.Find(e => e.Id == id)
               ?? throw new KeyNotFoundException($"Special event {id} not found.");
    }

    public void EnsureSaleAllowed(SpecialEvent ev, int ticketsSold, DateTime nowUtc)
    {
        if (nowUtc < ev.StartDate || nowUtc > ev.EndDate)
        {
            throw new InvalidOperationException("Visit date must be within the event window.");
        }

        if (ticketsSold >= ev.Capacity)
        {
            throw new InvalidOperationException("Event capacity reached.");
        }
    }

    public bool IsAttractionReferenced(Guid attractionId)
    {
        var matches = _repo.FindAll(
            ev => ev.Attractions.Any(att => att.Id == attractionId),
            e => e.Attractions);

        return matches?.Any() ?? false;
    }

    public IEnumerable<Guid> GetEventIdsByAttraction(Guid attractionId)
    {
        var events = _repo.FindAll(
            ev => ev.Attractions.Any(att => att.Id == attractionId),
            e => e.Attractions);

        return events.Select(e => e.Id);
    }
}
