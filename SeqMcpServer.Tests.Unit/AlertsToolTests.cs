using Seq.Api;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class AlertsToolTests : SdkToolTestBase
{
    protected override string ExpectedErrorSubstring => "Failed to get Seq alert state:";

    protected override Task<string> InvokeTool(SeqConnection connection, CancellationToken cancellationToken = default) =>
        AlertsTool.GetAlerts(connection, cancellationToken);
}
