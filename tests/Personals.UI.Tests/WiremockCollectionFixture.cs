using Personals.Tests.Base.Fixtures;

namespace Personals.UI.Tests;

[CollectionDefinition(nameof(WiremockCollectionFixture))]
public class WiremockCollectionFixture : ICollectionFixture<WiremockFixture>;