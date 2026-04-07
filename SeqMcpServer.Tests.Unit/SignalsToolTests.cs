using Seq.Api;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class SignalsToolTests : SdkToolTestBase
{
    protected override string ExpectedErrorSubstring => "Failed to list Seq signals:";

    protected override Task<string> InvokeTool(SeqConnection connection, CancellationToken cancellationToken = default) =>
        SignalsTool.ListSignals(connection, cancellationToken);
}
