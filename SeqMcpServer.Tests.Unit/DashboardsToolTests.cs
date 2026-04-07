using Seq.Api;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class DashboardsToolTests : SdkToolTestBase
{
    protected override string ExpectedErrorSubstring => "Failed to list Seq dashboards:";

    protected override Task<string> InvokeTool(SeqConnection connection, CancellationToken cancellationToken = default) =>
        DashboardsTool.ListDashboards(connection, cancellationToken);
}
