using System.Text.Json;
using Seq.Api;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class QueryLogsToolTests : SdkToolTestBase
{
    protected override string ExpectedErrorSubstring => "Failed to query Seq events:";

    protected override Task<string> InvokeTool(SeqConnection connection, CancellationToken cancellationToken = default) =>
        QueryLogsTool.QueryLogs(connection, cancellationToken: cancellationToken);

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
