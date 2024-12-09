using Personals.LookupTypes.Abstractions.Services;
using Personals.LookupTypes.Extensions;
using Personals.LookupTypes.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Personals.LookupTypes.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddLookupTypesModule_ShouldRegisterLookupTypesModule()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLookupTypesModule();

        // Assert
        var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(ILookupTypeService) &&
                                                       s.ImplementationType == typeof(LookupTypeService) &&
                                                       s.Lifetime == ServiceLifetime.Scoped);
        descriptor.Should().NotBeNull();

        descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(ILookupTypeSearchService) &&
                                                   s.ImplementationType == typeof(LookupTypeSearchService) &&
                                                   s.Lifetime == ServiceLifetime.Scoped);
        descriptor.Should().NotBeNull();
    }
}