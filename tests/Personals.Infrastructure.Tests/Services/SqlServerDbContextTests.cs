using Microsoft.Data.SqlClient;
using Personals.Infrastructure.Services;

namespace Personals.Infrastructure.Tests.Services;

public class SqlServerDbContextTests
{
    [Fact]
    public void Constructor_ShouldCreateDbContext()
    {
        // Arrange
        const string connectionString = "Server=localhost;Database=Personal;User Id=sa;Password=Password123!";

        // Act
        var dbContext = new SqlServerDbContext(connectionString);

        // Assert
        dbContext.Should().NotBeNull();
    }
    
    [Fact]
    public void GetConnection_ShouldReturnSqlConnection()
    {
        // Arrange
        const string connectionString = "Server=localhost;Database=Personal;User Id=sa;Password=Password123!";
        var dbContext = new SqlServerDbContext(connectionString);

        // Act
        var connection = dbContext.GetConnection();

        // Assert
        connection.Should().BeOfType<SqlConnection>();
        connection.ConnectionString.Should().Be(connectionString);
    }
}