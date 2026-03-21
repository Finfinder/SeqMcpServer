using System.Text.Json;
using Seq.Api;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class SignalsToolTests
{
    [Fact]
    public async Task ListSignals_CancelledToken_ReturnsJsonWithError()
    {
        var connection = new SeqConnection("http://localhost");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await SignalsTool.ListSignals(connection, cts.Token);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to list Seq signals:", error);
    }

    [Fact]
    public async Task ListSignals_ConnectionFailure_ReturnsJsonWithError()
    {
        var throwingHandler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        var connection = new SeqConnection("http://localhost", null, _ => throwingHandler);

        var result = await SignalsTool.ListSignals(connection);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to list Seq signals:", error);
    }
}
