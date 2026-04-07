using Seq.Api;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class RetentionPoliciesToolTests : SdkToolTestBase
{
    protected override string ExpectedErrorSubstring => "Failed to get Seq retention policies:";

    protected override Task<string> InvokeTool(SeqConnection connection, CancellationToken cancellationToken = default) =>
        RetentionPoliciesTool.GetRetentionPolicies(connection, cancellationToken);
}
