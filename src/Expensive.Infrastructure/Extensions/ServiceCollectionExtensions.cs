using Expensive.Infrastructure.Abstractions.Repositories;
using Expensive.Infrastructure.Abstractions.Services;
using Expensive.Infrastructure.Repositories;
using Expensive.Infrastructure.Services;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using TimeProvider = Expensive.Infrastructure.Services.TimeProvider;

namespace Expensive.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<ITimeProvider, TimeProvider>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRepositoryFactory, RepositoryFactory>();
        return services;
    }

    public static IServiceCollection AddDbContextAndMigrator(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDbContext>(_ => new SqlServerDbContext(connectionString));
        services.AddFluentMigratorCore()
            .ConfigureRunner(runnerBuilder => runnerBuilder
                .AddSqlServer()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(SqlServerDbContext).Assembly).For.Migrations())
            .AddLogging(loggingBuilder => loggingBuilder.AddFluentMigratorConsole());
        return services;
    }
}