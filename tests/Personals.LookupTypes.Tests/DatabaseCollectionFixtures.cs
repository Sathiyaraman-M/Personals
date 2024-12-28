using Personals.Tests.Base.Fixtures;

namespace Personals.LookupTypes.Tests;

[CollectionDefinition(nameof(DatabaseCollectionFixtures))]
public class DatabaseCollectionFixtures : ICollectionFixture<DatabaseFixture>;