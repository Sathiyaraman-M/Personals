using Expensive.Infrastructure.Abstractions.Utilities;
using Expensive.Infrastructure.Services;
using Expensive.Users.Utilities;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Expensive.Tests.Base;

public class MigrationRunner
{
    private readonly IServiceProvider _provider;

    private MigrationRunner(string connectionString)
    {
        _provider = new ServiceCollection()
            .AddScoped<IPasswordHasher, PasswordHasher>()
            .AddFluentMigratorCore()
            .ConfigureRunner(runnerBuilder => runnerBuilder
                .AddSqlServer()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(SqlServerDbContext).Assembly).For.Migrations())
            .BuildServiceProvider();
    }

    public static MigrationRunner Initialize(string connectionString) => new(connectionString);

    public void MigrateUp() => _provider.GetRequiredService<IMigrationRunner>().MigrateUp();
}