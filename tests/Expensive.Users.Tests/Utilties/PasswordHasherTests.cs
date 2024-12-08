using Expensive.Users.Utilities;

namespace Expensive.Users.Tests.Utilties;

public class PasswordHasherTests
{
    private const string PlaintextPassword = "my password";

    private const string HashedPassword =
        "AQAAAAIAAYagAAAAEAABAgMEBQYHCAkKCwwNDg/Q8A0WMKbtHQJQ2DHCdoEeeFBrgNlldq6vH4qX/CGqGQ==";

    [Fact]
    public void FullRoundTrip_SuccessCase()
    {
        // Arrange
        var hasher = new PasswordHasher();

        // Act
        var hashedPassword = hasher.HashPassword("password 1");
        var successResult = hasher.VerifyHashedPassword(hashedPassword, "password 1");

        // Assert
        successResult.Should().BeTrue();
    }

    [Fact]
    public void FullRoundTrip_FailureCase()
    {
        // Arrange
        var hasher = new PasswordHasher();

        // Act
        var hashedPassword = hasher.HashPassword("password 1");
        var failureResult = hasher.VerifyHashedPassword(hashedPassword, "password 2");

        // Assert
        failureResult.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_SuccessCase()
    {
        // Arrange
        var hasher = new PasswordHasher();

        // Act
        var hashedPassword = hasher.HashPassword(PlaintextPassword);

        // Assert
        hashedPassword.Should().NotBeNull()
            .And.NotBe(PlaintextPassword);
    }

    [Fact]
    public void VerifyHashedPassword_SuccessCase()
    {
        // Arrange
        var hasher = new PasswordHasher();

        // Act
        var result = hasher.VerifyHashedPassword(HashedPassword, PlaintextPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyHashedPassword_ShouldReturnFalse_WhenEmptyHash()
    {
        // Arrange
        var hasher = new PasswordHasher();

        // Act
        var result = hasher.VerifyHashedPassword(string.Empty, PlaintextPassword);

        // Assert
        result.Should().BeFalse();
    }
}