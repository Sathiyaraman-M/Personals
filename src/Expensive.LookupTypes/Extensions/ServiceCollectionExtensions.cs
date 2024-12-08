using Expensive.LookupTypes.Abstractions.Services;
using Expensive.LookupTypes.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Expensive.LookupTypes.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLookupTypesModule(this IServiceCollection services)
    {
        services.AddScoped<ILookupTypeService, LookupTypeService>();
        services.AddScoped<ILookupTypeSearchService, LookupTypeSearchService>();
        return services;
    }
}