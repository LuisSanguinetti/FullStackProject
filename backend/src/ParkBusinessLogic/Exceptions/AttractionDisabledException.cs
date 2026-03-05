namespace Park.BusinessLogic.Exceptions;

public class AttractionDisabledException : Exception
{
    public AttractionDisabledException(string name)
        : base($"Attraction: '{name}' is not enabled for users right now")
    {
    }
}
