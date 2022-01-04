namespace CF.Customer.Domain.Exceptions;

/// <summary>
///     Thrown when an entity cannot be found with a given id from the data layer
/// </summary>
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException()
    {
    }

    public EntityNotFoundException(string message) : base(message)
    {
    }

    public EntityNotFoundException(long id)
        : base($"Entity with id {id} was not found.")
    {
    }

    public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}