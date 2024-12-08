using Expensive.Infrastructure.Abstractions.Services;
using Expensive.Infrastructure.Extensions;
using Expensive.LookupTypes.Entities;
using Expensive.LookupTypes.Extensions;
using Expensive.Server.Permissions;
using Expensive.Server.Services;
using Expensive.Users.Entities;
using Expensive.Users.Extensions;
using Expensive.Users.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace Expensive.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        return services
            .AddCors()
            .AddInfrastructureServices()
            .AddPermissionBasedAuthorization()
            .AddDatabase(configuration)
            .AddCurrentUserService()
            .AddApplicationModules()
            .AddControllers().AddApplicationParts().Services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        services.AddDbContextAndMigrator(connectionString);
        return services;
    }

    public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }

    public static IServiceCollection AddPermissionBasedAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        return services;
    }

    private static IServiceCollection AddApplicationModules(this IServiceCollection services)
    {
        services.AddLookupTypesModule();
        services.AddUsersModule<JwtTokenConfiguration>();
        return services;
    }

    private static IMvcBuilder AddApplicationParts(this IMvcBuilder builder)
    {
        return builder
            .AddApplicationPart(typeof(AppUser).Assembly)
            .AddApplicationPart(typeof(LookupType).Assembly);
    }
}