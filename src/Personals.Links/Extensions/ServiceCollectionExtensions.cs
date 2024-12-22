using Microsoft.Extensions.DependencyInjection;
using Personals.Links.Abstractions.Services;
using Personals.Links.Services;

namespace Personals.Links.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLinksModule(this IServiceCollection services)
    {
        services.AddScoped<ILinkService, LinkService>();
        return services;
    }
}