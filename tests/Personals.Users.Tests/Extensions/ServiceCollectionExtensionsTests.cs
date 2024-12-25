using Personals.Infrastructure.Abstractions.Utilities;
using Personals.Tests.Base;
using Personals.Users.Abstractions.Services;
using Personals.Users.Abstractions.Utilities;
using Personals.Users.Extensions;
using Personals.Users.Services;
using Personals.Users.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Personals.Users.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    private sealed class StubJwtTokenConfiguration : IJwtTokenConfiguration
    {
        public string Secret => TestConstants.JwtSecret;
        public string Issuer => "Personals";
        public string Audience => "Personals";
        public int TokenExpirationInMinutes => 60;
        public int RefreshTokenExpirationInMinutes => 1440;
    }

    [Fact]
    public void AddUsersModule_ShouldRegisterUsersModule()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();
        services.AddSingleton(configuration);

        // Act
        services.AddUsersModule<StubJwtTokenConfiguration>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        AssertService<IUserService, UserService>(services, ServiceLifetime.Scoped);

        AssertService<IJwtTokenHandler, JwtTokenHandler>(services, ServiceLifetime.Scoped);
        AssertService<IPasswordHasher, PasswordHasher>(services, ServiceLifetime.Scoped);
        AssertService<ITokenService, TokenService>(services, ServiceLifetime.Scoped);
        AssertService<IJwtTokenConfiguration, StubJwtTokenConfiguration>(services, ServiceLifetime.Singleton);

        var authenticationSchemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        authenticationSchemeProvider.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme).Should().NotBeNull();

        var jwtTokenConfiguration = serviceProvider.GetRequiredService<IJwtTokenConfiguration>();
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
        var jwtBearerOptions = optionsMonitor.Get(JwtBearerDefaults.AuthenticationScheme);

        var tokenValidationParameters = jwtBearerOptions.TokenValidationParameters;
        tokenValidationParameters.ValidateIssuer.Should().Be(true);
        tokenValidationParameters.ValidateAudience.Should().Be(true);
        tokenValidationParameters.ValidateLifetime.Should().Be(true);
        tokenValidationParameters.ValidateIssuerSigningKey.Should().Be(true);
        tokenValidationParameters.ValidIssuer.Should().Be(jwtTokenConfiguration.Issuer);
        tokenValidationParameters.ValidAudience.Should().Be(jwtTokenConfiguration.Audience);
        tokenValidationParameters.IssuerSigningKey.Should().BeOfType<SymmetricSecurityKey>()
            .Which.Key.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(jwtTokenConfiguration.Secret));
    }

    private static void AssertService<TService, TImplementation>(IServiceCollection services, ServiceLifetime lifetime)
    {
        var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(TService) &&
                                                       s.ImplementationType == typeof(TImplementation) &&
                                                       s.Lifetime == lifetime);
        descriptor.Should().NotBeNull();
    }
}