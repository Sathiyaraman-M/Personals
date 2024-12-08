namespace Expensive.Infrastructure.Abstractions.Utilities;

public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}