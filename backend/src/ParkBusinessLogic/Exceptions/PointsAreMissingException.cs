namespace Park.BusinessLogic.Exceptions;
public class PointsAreMissingException : Exception
{
    public PointsAreMissingException(string name)
        : base($"User: {name}, you don't have the necessary points.")
    {
    }
}
