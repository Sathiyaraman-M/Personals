using Blazored.LocalStorage;
using Personals.Common.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Personals.UI.Abstractions.Services.Http;
using Personals.UI.Constants;
using Personals.UI.Services;
using Personals.UI.Services.Http;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace Personals.UI.Extensions;

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