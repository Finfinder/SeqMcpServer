using System.Text.Json;
using SeqMcpServer.Tests.Integration.Fixtures;
using SeqMcpServer.Tests.Integration.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Integration;

[Collection("Seq")]
public class DashboardsToolTests
{
    private readonly SeqContainerFixture _fixture;

    public DashboardsToolTests(SeqContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ListDashboards_ReturnsValidJsonArray()
    {
        var result = await DashboardsTool.ListDashboards(_fixture.Connection);

        var doc = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.Equal(JsonValueKind.Array, doc.ValueKind);
    }

    [Fact]
    public async Task ListDashboards_ResultDeserializesWithoutError()
    {
        var result = await DashboardsTool.ListDashboards(_fixture.Connection);

        ToolAssertions.AssertNoToolError(result);
    }
}
