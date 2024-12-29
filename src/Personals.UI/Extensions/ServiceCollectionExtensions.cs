using Blazored.LocalStorage;
using Personals.Common.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Personals.Common.Abstractions.Services;
using Personals.UI.Abstractions.Services;
using Personals.UI.Abstractions.Services.Http;
using Personals.UI.Constants;
using Personals.UI.Services;
using Personals.UI.Services.Http;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using TimeProvider = Personals.UI.Services.TimeProvider;

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
        services.AddTransient<ITimeProvider, TimeProvider>();
        return services;
    }

    private static void AddAuthentication(this IServiceCollection services)
    {
        services.AddTransient<IPersonalsAuthenticationStateProvider, PersonalsAuthenticationStateProvider>();
        services.AddTransient<AuthenticationStateProvider, PersonalsAuthenticationStateProvider>();
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
        services.AddTransient<ILinkService, LinkService>();
        services.AddTransient<ICodeSnippetService, CodeSnippetService>();
    }
}