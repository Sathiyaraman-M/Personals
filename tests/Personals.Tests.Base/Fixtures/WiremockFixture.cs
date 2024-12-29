using WireMock.Server;
using Xunit;

namespace Personals.Tests.Base.Fixtures;

public class WiremockFixture : IAsyncLifetime
{
    private readonly WireMockServer _server = WireMockServer.Start();
    
    public WireMockServer Server => _server;
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Server.Stop();
        return Task.CompletedTask;
    }
}