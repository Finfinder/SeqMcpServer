using System.Text.Json;
using SeqMcpServer.Tests.Integration.Fixtures;
using SeqMcpServer.Tests.Integration.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Integration;

[Collection("Seq")]
public class AlertsToolIntegrationTests
{
    private readonly SeqContainerFixture _fixture;

    public AlertsToolIntegrationTests(SeqContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAlerts_ReturnsValidJsonWithoutError()
    {
        var factory = new TestHttpClientFactory(_fixture.SeqUrl);

        var result = await AlertsTool.GetAlerts(factory);

        var doc = JsonSerializer.Deserialize<JsonElement>(result);
        Assert.NotEqual(JsonValueKind.Undefined, doc.ValueKind);
        Assert.False(doc.ValueKind == JsonValueKind.Object && doc.TryGetProperty("Error", out _),
            "Expected no Error in response");
    }

    [Fact]
    public async Task GetAlerts_ResultDeserializesSuccessfully()
    {
        var factory = new TestHttpClientFactory(_fixture.SeqUrl);

        var result = await AlertsTool.GetAlerts(factory);

        var exception = Record.Exception(() => JsonSerializer.Deserialize<JsonElement>(result));
        Assert.Null(exception);
    }
}
