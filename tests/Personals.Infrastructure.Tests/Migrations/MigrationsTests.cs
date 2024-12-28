using Dapper;
using FluentMigrator;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Personals.Infrastructure.Abstractions.Utilities;
using Personals.Infrastructure.Extensions;
using Personals.Infrastructure.Services;
using Personals.Tests.Base.Fixtures;
using Personals.Users.Utilities;

namespace Personals.Infrastructure.Tests.Migrations;

[Collection(nameof(DatabaseCollectionFixtures))]
public class MigrationsTests(DatabaseFixture databaseFixture)
{
    private SqlServerDbContext DbContext => new(databaseFixture.ConnectionString);
    
    [Fact]
    public async Task CanMigrateDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContextAndMigrator(databaseFixture.ConnectionString);
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        var serviceProvider = services.BuildServiceProvider();
        var migrationRunner = serviceProvider.GetRequiredService<IMigrationRunner>();
        
        var migrationClasses = typeof(SqlServerDbContext).Assembly.GetTypes()
            .Count(t => t.IsSubclassOf(typeof(MigrationBase)));

        // Act
        migrationRunner.MigrateUp();

        // Assert
        using var connection = DbContext.GetConnection();
        connection.Open();
        const string sql = "SELECT COUNT(*) FROM VersionInfo";
        var numberOfMigrations = await connection.QuerySingleAsync<int>(sql);
        numberOfMigrations.Should().Be(migrationClasses);
    }
}