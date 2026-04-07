using SeqMcpServer.Tests.Integration.Fixtures;
using SeqMcpServer.Tests.Integration.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Integration;

[Collection("Seq")]
public class DiagnosticsToolIntegrationTests
{
    private readonly SeqContainerFixture _fixture;

    public DiagnosticsToolIntegrationTests(SeqContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetDiagnostics_ReturnsNonEmptyResponse()
    {
        using var factory = new TestHttpClientFactory(_fixture.SeqUrl);

        var result = await DiagnosticsTool.GetDiagnostics(factory);

        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.DoesNotContain("Failed to get Seq diagnostics:", result);
    }

    [Fact]
    public async Task GetDiagnostics_DoesNotReturnError()
    {
        using var factory = new TestHttpClientFactory(_fixture.SeqUrl);

        var result = await DiagnosticsTool.GetDiagnostics(factory);

        Assert.DoesNotContain("\"Error\"", result);
    }
}
