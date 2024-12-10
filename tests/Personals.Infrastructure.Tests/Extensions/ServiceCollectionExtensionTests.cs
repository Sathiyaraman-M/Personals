using Microsoft.Extensions.DependencyInjection;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Extensions;
using Personals.Infrastructure.Repositories;
using TimeProvider = Personals.Infrastructure.Services.TimeProvider;

namespace Personals.Infrastructure.Tests.Extensions;

public class ServiceCollectionExtensionTests
{
    [Fact]
    public void AddInfrastructureServices_ShouldRegisterInfrastructureSpecificServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInfrastructureServices();

        // Assert
        var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IUnitOfWork) &&
                                                       s.ImplementationType == typeof(UnitOfWork) &&
                                                       s.Lifetime == ServiceLifetime.Scoped);
        descriptor.Should().NotBeNull(); 
        
        descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(ITimeProvider) &&
                                                   s.ImplementationType == typeof(TimeProvider) &&
                                                   s.Lifetime == ServiceLifetime.Singleton);
        descriptor.Should().NotBeNull();
        
        descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IRepositoryFactory) &&
                                                   s.ImplementationType == typeof(RepositoryFactory) &&
                                                   s.Lifetime == ServiceLifetime.Scoped);
        descriptor.Should().NotBeNull();
    }
}