using Microsoft.Extensions.DependencyInjection;
using Personals.CodeSnippets.Abstractions.Services;
using Personals.CodeSnippets.Extensions;
using Personals.CodeSnippets.Services;

namespace Personals.CodeSnippets.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCodeSnippetsModule_ShouldRegisterCodeSnippetModule()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCodeSnippetsModule();

        // Assert
        var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(ICodeSnippetService) &&
                                                       s.ImplementationType == typeof(CodeSnippetService) &&
                                                       s.Lifetime == ServiceLifetime.Scoped);
        descriptor.Should().NotBeNull();
        descriptor.Should().NotBeNull();
    }
}