namespace Expensive.Infrastructure.Exceptions;

public class EntityNotFoundException(
    string message = "Entity not found.",
    Exception? innerException = null) : Exception(message, innerException);