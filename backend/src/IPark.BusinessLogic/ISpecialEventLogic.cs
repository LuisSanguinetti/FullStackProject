using Domain;

namespace IParkBusinessLogic;

public interface ISpecialEventLogic
{
    public void EnsureSaleAllowed(SpecialEvent ev, int ticketsSold, DateTime nowUtc);
    public SpecialEvent GetOrThrow(Guid? id);
    bool IsAttractionReferenced(Guid attractionId);
    IEnumerable<Guid> GetEventIdsByAttraction(Guid attractionId);
}
