using System.Net;
using System.Text.Json;
using NSubstitute;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class AlertsToolTests
{
    [Fact]
    public async Task GetAlerts_SuccessfulResponse_ReturnsRawJson()
    {
        var expectedJson = """[{"Id":"alert-1","Title":"High error rate"}]""";
        var factory = CreateFactory(HttpStatusCode.OK, expectedJson);

        var result = await AlertsTool.GetAlerts(factory);

        Assert.Equal(expectedJson, result);
    }

    [Fact]
    public async Task GetAlerts_ServerError_ReturnsJsonWithError()
    {
        var factory = CreateFactory(HttpStatusCode.InternalServerError, "Internal Server Error");

        var result = await AlertsTool.GetAlerts(factory);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to get Seq alert state:", error);
    }

    [Fact]
    public async Task GetAlerts_HttpRequestException_ReturnsJsonWithError()
    {
        var factory = Substitute.For<IHttpClientFactory>();
        var throwingHandler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        var httpClient = new HttpClient(throwingHandler) { BaseAddress = new Uri("http://localhost") };
        factory.CreateClient("Seq").Returns(httpClient);

        var result = await AlertsTool.GetAlerts(factory);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to get Seq alert state:", error);
    }

    private static IHttpClientFactory CreateFactory(HttpStatusCode statusCode, string content)
    {
        var handler = new MockHttpMessageHandler(statusCode, content);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("Seq").Returns(httpClient);
        return factory;
    }
}
