using System.Text.Json;
using Seq.Api;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class SqlQueryToolTests
{
    [Fact]
    public async Task RunSql_CancelledToken_ReturnsJsonWithError()
    {
        using var connection = new SeqConnection("http://localhost");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await SqlQueryTool.RunSql(connection, "select 1", cancellationToken: cts.Token);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to execute Seq SQL query:", error);
    }

    [Fact]
    public async Task RunSql_ConnectionFailure_ReturnsJsonWithError()
    {
        var throwingHandler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        using var connection = new SeqConnection("http://localhost", null, _ => throwingHandler);

        var result = await SqlQueryTool.RunSql(connection, "select 1");

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to execute Seq SQL query:", error);
    }

    [Fact]
    public async Task RunSql_InvalidFromUtcFormat_ReturnsJsonWithError()
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await SqlQueryTool.RunSql(connection, "select 1", fromUtc: "not-a-date");

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to execute Seq SQL query:", error);
    }

    [Fact]
    public async Task RunSql_InvalidToUtcFormat_ReturnsJsonWithError()
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await SqlQueryTool.RunSql(connection, "select 1", toUtc: "not-a-date");

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to execute Seq SQL query:", error);
    }
}
