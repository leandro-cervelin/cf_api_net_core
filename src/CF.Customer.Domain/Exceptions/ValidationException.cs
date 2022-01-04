namespace CF.Customer.Domain.Exceptions;

/// <summary>
///     Thrown when an entity cannot be found with a given id from the data layer
/// </summary>
public class ValidationException : Exception
{
    public ValidationException()
    {
    }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}