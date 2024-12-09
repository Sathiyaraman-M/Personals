using Testcontainers.MsSql;
using Xunit;

namespace Personals.Tests.Base;

public class DatabaseFixture : IAsyncLifetime
{
    private MsSqlContainer Container { get; } = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-CU10-ubuntu-22.04")
        .Build();

    public string ConnectionString => Container.GetConnectionString();

    public virtual async Task InitializeAsync()
    {
        await Container.StartAsync();
        MigrationRunner.Initialize(ConnectionString).MigrateUp();
    }

    public virtual async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync();
    }
}