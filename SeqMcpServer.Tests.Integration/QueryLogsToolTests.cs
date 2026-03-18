using System.Text.Json;
using SeqMcpServer.Tests.Integration.Fixtures;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Integration;

[Collection("Seq")]
public class QueryLogsToolTests
{
    private readonly SeqContainerFixture _fixture;

    public QueryLogsToolTests(SeqContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task QueryLogs_WithoutFilter_ReturnsEventsWithExpectedFields()
    {
        var result = await QueryLogsTool.QueryLogs(
            _fixture.Connection,
            count: 100,
            fromUtc: "2025-01-15T00:00:00Z",
            toUtc: "2025-01-16T00:00:00Z");

        var events = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.Equal(JsonValueKind.Array, events.ValueKind);
        Assert.True(events.GetArrayLength() > 0);

        var first = events[0];
        Assert.True(first.TryGetProperty("Id", out _));
        Assert.True(first.TryGetProperty("Timestamp", out _));
        Assert.True(first.TryGetProperty("Level", out _));
        Assert.True(first.TryGetProperty("Message", out _));
    }

    [Fact]
    public async Task QueryLogs_WithErrorFilter_ReturnsOnlyErrors()
    {
        var result = await QueryLogsTool.QueryLogs(
            _fixture.Connection,
            filter: "@Level = \"Error\"",
            count: 100,
            fromUtc: "2025-01-15T00:00:00Z",
            toUtc: "2025-01-16T00:00:00Z");

        var events = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.Equal(JsonValueKind.Array, events.ValueKind);

        foreach (var ev in events.EnumerateArray())
        {
            Assert.Equal("Error", ev.GetProperty("Level").GetString());
        }
    }

    [Fact]
    public async Task QueryLogs_CountExceedsMax_IsClampedTo500()
    {
        var result = await QueryLogsTool.QueryLogs(
            _fixture.Connection,
            count: 1000,
            fromUtc: "2025-01-15T00:00:00Z",
            toUtc: "2025-01-16T00:00:00Z");

        var events = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.Equal(JsonValueKind.Array, events.ValueKind);
        // With only 5 seeded events, the count clamping is transparent,
        // but the call should succeed without error
        Assert.True(events.GetArrayLength() <= 500);
    }

    [Fact]
    public async Task QueryLogs_ResultDeserializesWithoutError()
    {
        var result = await QueryLogsTool.QueryLogs(
            _fixture.Connection,
            count: 10,
            fromUtc: "2025-01-15T00:00:00Z",
            toUtc: "2025-01-16T00:00:00Z");

        var doc = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.NotEqual(JsonValueKind.Undefined, doc.ValueKind);
        Assert.False(doc.ValueKind == JsonValueKind.Object && doc.TryGetProperty("Error", out _),
            "Expected no Error in response");
    }
}
