using Domain;

namespace IParkBusinessLogic;

public interface IAccessRecordLogic
{
    public int? Register(Guid ticketQr, Guid attractionId, DateTime nowUtc, Guid scoringStrategyId);
    public IList<AccessRecord> FindByAttractionAndDate(Guid attractionId, DateTime startDate, DateTime endDate);
    public bool ValidateAge(Guid ticketQr, Guid attractionId);
    public int CheckCurrentCapacity(Guid attractionId);
    public bool ValidateMaximumCapacity(Guid attractionId);
    public void RegisterExit(Guid accessRecordId, DateTime nowUtc);
    public int RemainingPeopleCapacity(Guid attractionId);
    public bool ValidateSpecialEvent(Guid ticketQr, Guid attractionId, IEnumerable<Guid> specialEventIds);
}
