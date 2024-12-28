using Personals.Tests.Base.Fixtures;

namespace Personals.CodeSnippets.Tests;

[CollectionDefinition(nameof(DatabaseCollectionFixtures))]
public class DatabaseCollectionFixtures : ICollectionFixture<DatabaseFixture>;