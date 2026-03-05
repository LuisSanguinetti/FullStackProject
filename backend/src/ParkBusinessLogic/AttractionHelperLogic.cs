using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class AttractionHelperLogic : IAttractionHelperLogic
{
    private readonly IRepository<Attraction> _attractionRepository;

    public AttractionHelperLogic(IRepository<Attraction> attractionRepository)
    {
        _attractionRepository = attractionRepository;
    }

    public Attraction GetOrThrow(Guid id)
    {
        return _attractionRepository.Find(a => a.Id == id)
               ?? throw new KeyNotFoundException($"Attraction {id} not found.");
    }
}
