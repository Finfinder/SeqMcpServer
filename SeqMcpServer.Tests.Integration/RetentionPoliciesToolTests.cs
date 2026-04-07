using System.Text.Json;
using SeqMcpServer.Tests.Integration.Fixtures;
using SeqMcpServer.Tests.Integration.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Integration;

[Collection("Seq")]
public class RetentionPoliciesToolTests
{
    private readonly SeqContainerFixture _fixture;

    public RetentionPoliciesToolTests(SeqContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetRetentionPolicies_ReturnsValidJsonArray()
    {
        var result = await RetentionPoliciesTool.GetRetentionPolicies(_fixture.Connection);

        var doc = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.Equal(JsonValueKind.Array, doc.ValueKind);
    }

    [Fact]
    public async Task GetRetentionPolicies_ResultDeserializesWithoutError()
    {
        var result = await RetentionPoliciesTool.GetRetentionPolicies(_fixture.Connection);

        ToolAssertions.AssertNoToolError(result);
    }
}
