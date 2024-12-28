using Personals.Tests.Base.Fixtures;

namespace Personals.Links.Tests;

[CollectionDefinition(nameof(DatabaseCollectionFixtures))]
public class DatabaseCollectionFixtures : ICollectionFixture<DatabaseFixture>;