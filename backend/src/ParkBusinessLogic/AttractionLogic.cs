using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

// aca hay 2 para evitar dependencias circulares
public class AttractionLogic : IAttractionLogic
{
    private readonly IAccessRecordLogic _accessLogic;
    private readonly IRepository<Attraction> _attractionRepository;
    public AttractionLogic(IAccessRecordLogic accessLogic, IRepository<Attraction> attractionRepository)
    {
        _accessLogic = accessLogic;
        _attractionRepository = attractionRepository;
    }

    public int NumberOfVisits(Guid id, DateTime startDate, DateTime endDate)
    {
            Attraction attraction = _attractionRepository.Find(a => a.Id == id);

            if(attraction is null)
            {
                throw new KeyNotFoundException($"Attraction with ID {id} not found.");
            }

            IList<AccessRecord> listAccessRecord = _accessLogic.FindByAttractionAndDate(attraction.Id, startDate, endDate);

            return listAccessRecord.Count;
  }

    public Attraction GetOrThrow(Guid id)
    {
        return _attractionRepository.Find(a => a.Id == id)
               ?? throw new KeyNotFoundException($"Attraction {id} not found.");
    }

    public List<Attraction> GetAll()
    {
        return _attractionRepository.FindAll().ToList();
    }
}
