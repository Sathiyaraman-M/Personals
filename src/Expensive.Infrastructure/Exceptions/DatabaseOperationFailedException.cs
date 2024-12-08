namespace Expensive.Infrastructure.Exceptions;

public class DatabaseOperationFailedException(
    string message = "An error occurred while performing a database operation.",
    Exception? innerException = null) : Exception(message, innerException);