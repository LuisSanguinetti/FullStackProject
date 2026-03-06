namespace Park.BusinessLogic.Exceptions;

public class AttractionCapacityReachedException  : Exception
{
    public AttractionCapacityReachedException(string name, int maxCapacity)
        : base($"Attraction '{name}' is at capacity ({maxCapacity}")
    {
    }
}
