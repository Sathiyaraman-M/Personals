using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Extensions;
using Personals.LookupTypes.Entities;
using Personals.LookupTypes.Extensions;
using Personals.Users.Entities;
using Personals.Users.Extensions;
using Personals.Users.Utilities;
using Microsoft.AspNetCore.Authorization;
using Personals.Server.Permissions;
using Personals.Server.Services;

namespace Personals.Server.Extensions;

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