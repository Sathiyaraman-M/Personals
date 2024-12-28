using Personals.Tests.Base.Fixtures;

namespace Personals.Users.Tests;

[CollectionDefinition(nameof(DatabaseCollectionFixtures))]
public class DatabaseCollectionFixtures : ICollectionFixture<DatabaseFixture>;