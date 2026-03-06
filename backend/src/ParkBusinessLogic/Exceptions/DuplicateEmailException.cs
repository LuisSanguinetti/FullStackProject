namespace Park.BusinessLogic.Exceptions;

public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email)
        : base($"Email '{email}' is already registered.")
    {
    }
}
