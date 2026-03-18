using System.Text.Json;
using SeqMcpServer.Tests.Integration.Fixtures;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Integration;

[Collection("Seq")]
public class SignalsToolTests
{
    private readonly SeqContainerFixture _fixture;

    public SignalsToolTests(SeqContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ListSignals_ReturnsAtLeastOneSignal()
    {
        var result = await SignalsTool.ListSignals(_fixture.Connection);

        var signals = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.Equal(JsonValueKind.Array, signals.ValueKind);
        Assert.True(signals.GetArrayLength() >= 1, "Expected at least one seeded signal");
    }

    [Fact]
    public async Task ListSignals_SignalHasExpectedFields()
    {
        var result = await SignalsTool.ListSignals(_fixture.Connection);

        var signals = JsonSerializer.Deserialize<JsonElement>(result);
        var first = signals[0];
        Assert.True(first.TryGetProperty("Id", out _));
        Assert.True(first.TryGetProperty("Title", out _));
    }

    [Fact]
    public async Task ListSignals_ResultDeserializesWithoutError()
    {
        var result = await SignalsTool.ListSignals(_fixture.Connection);

        var doc = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.False(doc.ValueKind == JsonValueKind.Object && doc.TryGetProperty("Error", out _),
            "Expected no Error in response");
    }
}
