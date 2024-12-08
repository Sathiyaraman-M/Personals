namespace Expensive.UI.Exceptions;

public class LoginFailedException(string? message = "Login failed") : Exception(message);