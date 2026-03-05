using Domain;

namespace IParkBusinessLogic;

public interface IAttractionHelperLogic
{
    Attraction GetOrThrow(Guid id);
}
