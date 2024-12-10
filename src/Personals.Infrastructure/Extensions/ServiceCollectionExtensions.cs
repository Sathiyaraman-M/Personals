using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Repositories;
using Personals.Infrastructure.Services;
using TimeProvider = Personals.Infrastructure.Services.TimeProvider;

namespace Personals.Infrastructure.Extensions;

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