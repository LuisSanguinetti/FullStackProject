using Domain;

namespace IParkBusinessLogic;
public interface IAttractionLogic
{
    public int NumberOfVisits(Guid id, DateTime startDate, DateTime endDate);
    public Attraction GetOrThrow(Guid id);
    public List<Attraction> GetAll();
}
