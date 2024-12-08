using Blazored.LocalStorage;
using Expensive.Common.Constants;
using Expensive.UI.Abstractions.Services.Http;
using Expensive.UI.Constants;
using Expensive.UI.Services;
using Expensive.UI.Services.Http;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Expensive.UI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, string baseAddress)
    {
        services.AddSingleton<AppThemeService>();
        services.AddMudServices();
        services.AddAuthentication();
        services.AddHttpServices(baseAddress);
        services.AddHttpClientInterceptor();
        services.AddBlazoredLocalStorage();
        return services;
    }

    private static void AddAuthentication(this IServiceCollection services)
    {
        services.AddTransient<PayFlowAuthenticationStateProvider>();
        services.AddTransient<AuthenticationStateProvider, PayFlowAuthenticationStateProvider>();
        services.AddAuthorizationCore(options =>
        {
            foreach (var permission in Permissions.GetAllPermissions())
            {
                options.AddPolicy(permission, policy => policy.RequireClaim(ApplicationClaimTypes.Permission, permission));
            }
        });
        services.AddTransient<AuthenticationHeaderHandler>();
        services.AddTransient<HttpAuthorizationInterceptor>();
    }

    private static void AddHttpServices(this IServiceCollection services, string baseAddress)
    {
        services.AddHttpClient(ApplicationConstants.HttpClientName, client => client.BaseAddress = new Uri(baseAddress))
            .AddHttpMessageHandler<AuthenticationHeaderHandler>();
        services.AddScoped(serviceProvider => serviceProvider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(ApplicationConstants.HttpClientName)
            .EnableIntercept(serviceProvider));
        services.AddTransient<ILookupTypeService, LookupTypeService>();
        services.AddTransient<ITokenService, TokenService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<ISearchService, SearchService>();
    }
}