namespace Park.BusinessLogic.Exceptions;

public class AgeRequirementNotMetException : Exception
{
    public AgeRequirementNotMetException(string name)
        : base($"User: {name}, does not meet the required age")
    {
    }
}
