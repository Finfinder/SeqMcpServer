using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Seq.Api;
using SeqMcpServer.Tests.Integration.Helpers;

namespace SeqMcpServer.Tests.Integration.Fixtures;

public class SeqContainerFixture : IAsyncLifetime
{
    private const ushort SeqPort = 80;

    private readonly IContainer _container = new ContainerBuilder()
        .WithImage("datalust/seq:latest")
        .WithPortBinding(SeqPort, true)
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithEnvironment("SEQ_FIRSTRUN_NOAUTHENTICATION", "true")
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(r => r.ForPort(SeqPort).ForPath("/api")))
        .WithCleanUp(true)
        .Build();

    public string SeqUrl => $"http://{_container.Hostname}:{_container.GetMappedPublicPort(SeqPort)}";

    public SeqConnection Connection { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Connection = new SeqConnection(SeqUrl);

        await SeqTestDataHelper.SeedEventsAsync(SeqUrl);
        await SeqTestDataHelper.WaitForEventsIndexedAsync(Connection);
        await SeqTestDataHelper.SeedSignalAsync(Connection);
    }

    public async Task DisposeAsync()
    {
        Connection.Dispose();
        await _container.DisposeAsync();
    }
}
