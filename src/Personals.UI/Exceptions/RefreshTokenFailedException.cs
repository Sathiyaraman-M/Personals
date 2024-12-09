namespace Personals.UI.Exceptions;

public class RefreshTokenFailedException(string? message = "Refresh token failed") : Exception(message);