using System.Net;
using System.Text.Json;
using NSubstitute;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class DiagnosticsToolTests
{
    [Fact]
    public async Task GetDiagnostics_SuccessfulResponse_ReturnsRawJson()
    {
        var expectedJson = """{"Status":"Running","Uptime":"01:30:00"}""";
        var factory = HttpClientFactoryHelper.CreateFactory(HttpStatusCode.OK, expectedJson);

        var result = await DiagnosticsTool.GetDiagnostics(factory);

        Assert.Equal(expectedJson, result);
    }

    [Fact]
    public async Task GetDiagnostics_ServerError_ReturnsJsonWithError()
    {
        var factory = HttpClientFactoryHelper.CreateFactory(HttpStatusCode.InternalServerError, "Internal Server Error");

        var result = await DiagnosticsTool.GetDiagnostics(factory);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to get Seq diagnostics:", error);
    }

    [Fact]
    public async Task GetDiagnostics_HttpRequestException_ReturnsJsonWithError()
    {
        var factory = Substitute.For<IHttpClientFactory>();
        var throwingHandler = new ThrowingHttpMessageHandler(new HttpRequestException("Connection refused"));
        var httpClient = new HttpClient(throwingHandler) { BaseAddress = new Uri("http://localhost") };
        factory.CreateClient("Seq").Returns(httpClient);

        var result = await DiagnosticsTool.GetDiagnostics(factory);

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to get Seq diagnostics:", error);
    }
}
