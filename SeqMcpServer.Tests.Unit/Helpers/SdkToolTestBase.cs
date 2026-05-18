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
        await cts.CancelAsync();

        var result = await InvokeTool(connection, cts.Token);

        ToolAssertions.AssertJsonError(result, ExpectedErrorSubstring);
    }

    [Fact]
    public async Task ConnectionFailure_ReturnsJsonWithError()
    {
        var throwingHandler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        using var connection = new SeqConnection("http://localhost", null, _ => throwingHandler);

        var result = await InvokeTool(connection);

        ToolAssertions.AssertJsonError(result, ExpectedErrorSubstring);
    }
}
