using Personals.Tests.Base.Fixtures;

namespace Personals.Infrastructure.Tests;

[CollectionDefinition(nameof(DatabaseCollectionFixtures))]
public class DatabaseCollectionFixtures : ICollectionFixture<DatabaseFixture>;