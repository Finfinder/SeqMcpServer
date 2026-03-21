using System.Text.Json;
using Seq.Api;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class RetentionPoliciesToolTests
{
    [Fact]
    public async Task GetRetentionPolicies_CancelledToken_ReturnsJsonWithError()
    {
        var connection = new SeqConnection("http://localhost");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await RetentionPoliciesTool.GetRetentionPolicies(connection, cts.Token);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to get Seq retention policies:", error);
    }

    [Fact]
    public async Task GetRetentionPolicies_ConnectionFailure_ReturnsJsonWithError()
    {
        var throwingHandler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        var connection = new SeqConnection("http://localhost", null, _ => throwingHandler);

        var result = await RetentionPoliciesTool.GetRetentionPolicies(connection);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to get Seq retention policies:", error);
    }
}
