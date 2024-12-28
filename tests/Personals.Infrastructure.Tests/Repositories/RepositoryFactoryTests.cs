using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Personals.Infrastructure.Abstractions.Entities;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Extensions;
using Personals.Infrastructure.Repositories;
using Personals.LookupTypes.Entities;
using Personals.LookupTypes.Extensions;
using Personals.LookupTypes.Repositories;
using Personals.Tests.Base.Fixtures;

namespace Personals.Infrastructure.Tests.Repositories;

public sealed record SomeEntity : IEntity;

public interface ISomeRepository : IRepository<SomeEntity>;

public sealed class SomeRepository : ISomeRepository;

[Collection(nameof(DatabaseCollectionFixtures))]
public class RepositoryFactoryTests(DatabaseFixture databaseFixture)
{
    [Fact]
    public void CreateRepository_ShouldCreateRepository()
    {
        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddInfrastructureServices()
            .AddLookupTypesModule()
            .AddLogging()
            .AddScoped<ICurrentUserService>(_ => Substitute.For<ICurrentUserService>())
            .BuildServiceProvider();
        
        using var connection = new SqlConnection(databaseFixture.ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        
        var factory = new RepositoryFactory(serviceProvider);

        // Act
        var repository = factory.CreateRepository<LookupType, LookupTypeRepository>(connection, transaction);

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeOfType<LookupTypeRepository>();
        repository.Should().BeAssignableTo<IRepository<LookupType>>();
    }
}