using System.Text.Json;
using Seq.Api;

namespace SeqMcpServer.Tests.Unit.Helpers;

public abstract class SdkToolTestBase
{
    protected abstract string ExpectedErrorSubstring { get; }

    protected abstract Task<string> InvokeTool(SeqConnection connection, CancellationToken cancellationToken = default);

    [Fact]
    public async Task CancelledToken_ReturnsJsonWithError()
    {
        using var connection = new SeqConnection("http://localhost");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await InvokeTool(connection, cts.Token);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains(ExpectedErrorSubstring, error);
    }

    [Fact]
    public async Task ConnectionFailure_ReturnsJsonWithError()
    {
        var throwingHandler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        using var connection = new SeqConnection("http://localhost", null, _ => throwingHandler);

        var result = await InvokeTool(connection);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains(ExpectedErrorSubstring, error);
    }
}
