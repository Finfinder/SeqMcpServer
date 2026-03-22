using System.Text.Json;
using Seq.Api;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class AlertsToolTests
{
    [Fact]
    public async Task GetAlerts_CancelledToken_ReturnsJsonWithError()
    {
        var connection = new SeqConnection("http://localhost");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await AlertsTool.GetAlerts(connection, cts.Token);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to get Seq alert state:", error);
    }

    [Fact]
    public async Task GetAlerts_ConnectionFailure_ReturnsJsonWithError()
    {
        var throwingHandler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        var connection = new SeqConnection("http://localhost", null, _ => throwingHandler);

        var result = await AlertsTool.GetAlerts(connection);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to get Seq alert state:", error);
    }
}
