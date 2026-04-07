using System.Text.Json;
using Seq.Api;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class SqlQueryToolTests : SdkToolTestBase
{
    protected override string ExpectedErrorSubstring => "Failed to execute Seq SQL query:";

    protected override Task<string> InvokeTool(SeqConnection connection, CancellationToken cancellationToken = default) =>
        SqlQueryTool.RunSql(connection, "select 1", cancellationToken: cancellationToken);

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public async Task RunSql_EmptyOrWhitespaceQuery_ReturnsJsonWithError(string query)
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await SqlQueryTool.RunSql(connection, query);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Equal("Query cannot be empty.", error);
    }

    [Fact]
    public async Task RunSql_InvalidFromUtcFormat_ReturnsJsonWithError()
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await SqlQueryTool.RunSql(connection, "select 1", fromUtc: "not-a-date");

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to execute Seq SQL query:", error);
        Assert.Contains("fromUtc", error);
    }

    [Fact]
    public async Task RunSql_InvalidToUtcFormat_ReturnsJsonWithError()
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await SqlQueryTool.RunSql(connection, "select 1", toUtc: "not-a-date");

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to execute Seq SQL query:", error);
        Assert.Contains("toUtc", error);
    }

    [Theory]
    [InlineData("select rate_limit from stream")]
    [InlineData("select * from stream -- no limit")]
    [InlineData("select * from stream where msg = 'speed limit'")]
    [InlineData("select x AS limited_data from stream")]
    [InlineData("select * from stream")]
    public void HasLimitClause_QueryWithoutLimitClause_ReturnsFalse(string query)
    {
        Assert.False(SqlQueryTool.HasLimitClause(query));
    }

    [Theory]
    [InlineData("select * from stream limit 10")]
    [InlineData("select * from stream LIMIT 100")]
    [InlineData("select * from stream limit 10 offset 5")]
    [InlineData("select * from stream\nLIMIT 50")]
    public void HasLimitClause_QueryWithLimitClause_ReturnsTrue(string query)
    {
        Assert.True(SqlQueryTool.HasLimitClause(query));
    }

    [Fact]
    public void HasLimitClause_EmptyString_ReturnsFalse()
    {
        Assert.False(SqlQueryTool.HasLimitClause(""));
    }

    [Fact]
    public void HasLimitClause_LimitKeywordWithoutNumber_ReturnsFalse()
    {
        Assert.False(SqlQueryTool.HasLimitClause("select * from stream limit"));
    }

    [Fact]
    public void HasLimitClause_LimitZero_ReturnsTrue()
    {
        // "limit 0" matches \bLIMIT\s+\d+ because 0 is a digit
        Assert.True(SqlQueryTool.HasLimitClause("select * from stream limit 0"));
    }

    [Fact]
    public async Task RunSql_InvalidBothDates_ReportsFromUtcFirst()
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await SqlQueryTool.RunSql(connection, "select 1", fromUtc: "bad-from", toUtc: "bad-to");

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("fromUtc", error);
        Assert.Contains("bad-from", error);
    }
}
