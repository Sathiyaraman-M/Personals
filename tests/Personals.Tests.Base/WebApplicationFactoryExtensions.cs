using Personals.Infrastructure.Abstractions.Services;
using Personals.Server;
using Personals.Server.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Personals.Tests.Base.Services;
using System.Net.Http.Headers;

namespace Personals.Tests.Base;

public static class WebApplicationFactoryExtensions
{
    public static WebApplicationFactory<Program> GetCustomWebApplicationFactory(
        this WebApplicationFactory<Program> factory, Action<IServiceCollection>? configureServices = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddTraditionalFormatEnvironmentVariables()
            .AddInMemoryCollection([new KeyValuePair<string, string?>("JWT_SECRET", TestConstants.JwtSecret)])
            .Build();
        return factory.WithWebHostBuilder(builder =>
        {
            builder.UseConfiguration(configuration);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITimeProvider>();
                services.AddScoped<ITimeProvider, StubTimeProvider>();
                configureServices?.Invoke(services);
            });
        });
    }

    public static HttpClient CreateClientWithJwtBearer(this WebApplicationFactory<Program> factory, string jwtBearer)
    {
        var httpClient = factory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtBearer);
        return httpClient;
    }
}