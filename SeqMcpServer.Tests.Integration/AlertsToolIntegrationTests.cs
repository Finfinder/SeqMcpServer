using System.Text.Json;
using SeqMcpServer.Tests.Integration.Fixtures;
using SeqMcpServer.Tests.Integration.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Integration;

[Collection("Seq")]
public class AlertsToolIntegrationTests
{
    private readonly SeqContainerFixture _fixture;

    public AlertsToolIntegrationTests(SeqContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAlerts_ReturnsValidJsonWithoutError()
    {
        var result = await AlertsTool.GetAlerts(_fixture.Connection);

        var doc = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.Equal(JsonValueKind.Array, doc.ValueKind);
    }

    [Fact]
    public async Task GetAlerts_ResultDeserializesWithoutError()
    {
        var result = await AlertsTool.GetAlerts(_fixture.Connection);

        ToolAssertions.AssertNoToolError(result);
    }
}
