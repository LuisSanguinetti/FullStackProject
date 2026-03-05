namespace Park.BusinessLogic.Exceptions;

public class SpecialEventAttractionMismatchException : Exception
{
    public SpecialEventAttractionMismatchException(string name, string eventName)
        : base($"Attraction '{name}' is not part of special event '{eventName}'.")
    {
    }
}
