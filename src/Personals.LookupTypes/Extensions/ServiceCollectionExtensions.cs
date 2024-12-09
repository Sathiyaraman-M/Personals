using Microsoft.Extensions.DependencyInjection;
using Personals.LookupTypes.Abstractions.Services;
using Personals.LookupTypes.Services;

namespace Personals.LookupTypes.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLookupTypesModule(this IServiceCollection services)
    {
        services.AddScoped<ILookupTypeService, LookupTypeService>();
        services.AddScoped<ILookupTypeSearchService, LookupTypeSearchService>();
        return services;
    }
}