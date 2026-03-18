using System.Text.Json;
using SeqMcpServer.Tests.Integration.Fixtures;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Integration;

[Collection("Seq")]
public class SqlQueryToolTests
{
    private readonly SeqContainerFixture _fixture;

    public SqlQueryToolTests(SeqContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RunSql_CountQuery_ReturnsColumnsAndRows()
    {
        var result = await SqlQueryTool.RunSql(
            _fixture.Connection,
            query: "select count(*) from stream",
            fromUtc: "2025-01-15T00:00:00Z",
            toUtc: "2025-01-16T00:00:00Z");

        var doc = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.True(doc.TryGetProperty("Columns", out var columns));
        Assert.True(doc.TryGetProperty("Rows", out var rows));
        Assert.True(columns.GetArrayLength() > 0);
        Assert.True(rows.GetArrayLength() > 0);
    }

    [Fact]
    public async Task RunSql_QueryWithoutLimit_AppendsLimitAutomatically()
    {
        var result = await SqlQueryTool.RunSql(
            _fixture.Connection,
            query: "select * from stream",
            fromUtc: "2025-01-15T00:00:00Z",
            toUtc: "2025-01-16T00:00:00Z");

        var doc = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.False(doc.TryGetProperty("Error", out _), "Expected no Error in response");
        Assert.True(doc.TryGetProperty("Columns", out _));
        Assert.True(doc.TryGetProperty("Rows", out _));
    }

    [Fact]
    public async Task RunSql_QueryWithExistingLimit_DoesNotAppendDuplicateLimit()
    {
        var result = await SqlQueryTool.RunSql(
            _fixture.Connection,
            query: "select * from stream limit 10",
            fromUtc: "2025-01-15T00:00:00Z",
            toUtc: "2025-01-16T00:00:00Z");

        var doc = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.False(doc.TryGetProperty("Error", out _), "Expected no Error in response");
        Assert.True(doc.TryGetProperty("Rows", out var rows));
        Assert.True(rows.GetArrayLength() <= 10);
    }

    [Fact]
    public async Task RunSql_ResultDeserializesWithoutError()
    {
        var result = await SqlQueryTool.RunSql(
            _fixture.Connection,
            query: "select count(*) from stream group by @Level",
            fromUtc: "2025-01-15T00:00:00Z",
            toUtc: "2025-01-16T00:00:00Z");

        var doc = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.NotEqual(JsonValueKind.Undefined, doc.ValueKind);
        Assert.False(doc.TryGetProperty("Error", out _), "Expected no Error in response");
    }
}
