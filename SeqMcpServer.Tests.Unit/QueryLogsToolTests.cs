using System.Text.Json;
using Seq.Api;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class QueryLogsToolTests
{
    [Fact]
    public async Task QueryLogs_CancelledToken_ReturnsJsonWithError()
    {
        using var connection = new SeqConnection("http://localhost");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await QueryLogsTool.QueryLogs(connection, cancellationToken: cts.Token);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to query Seq events:", error);
    }

    [Fact]
    public async Task QueryLogs_ConnectionFailure_ReturnsJsonWithError()
    {
        var throwingHandler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        using var connection = new SeqConnection("http://localhost", null, _ => throwingHandler);

        var result = await QueryLogsTool.QueryLogs(connection);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to query Seq events:", error);
    }

    [Fact]
    public async Task QueryLogs_InvalidFromUtcFormat_ReturnsJsonWithError()
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await QueryLogsTool.QueryLogs(connection, fromUtc: "not-a-date");

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to query Seq events:", error);
        Assert.Contains("fromUtc", error);
    }

    [Fact]
    public async Task QueryLogs_InvalidToUtcFormat_ReturnsJsonWithError()
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await QueryLogsTool.QueryLogs(connection, toUtc: "not-a-date");

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to query Seq events:", error);
        Assert.Contains("toUtc", error);
    }
}
