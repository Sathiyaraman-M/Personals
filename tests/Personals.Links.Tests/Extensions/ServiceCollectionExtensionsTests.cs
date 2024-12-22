using Personals.Links.Abstractions.Services;
using Personals.Links.Extensions;
using Personals.Links.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Personals.Links.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddLinksModule_ShouldRegisterLinkTypesModule()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLinksModule();

        // Assert
        var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(ILinkService) &&
                                                       s.ImplementationType == typeof(LinkService) &&
                                                       s.Lifetime == ServiceLifetime.Scoped);
        descriptor.Should().NotBeNull();
        descriptor.Should().NotBeNull();
    }
}