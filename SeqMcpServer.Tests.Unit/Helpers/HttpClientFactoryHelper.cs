using System.Net;
using NSubstitute;

namespace SeqMcpServer.Tests.Unit.Helpers;

internal static class HttpClientFactoryHelper
{
    public static IHttpClientFactory CreateFactory(HttpStatusCode statusCode, string content)
    {
        var handler = new MockHttpMessageHandler(statusCode, content);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("Seq").Returns(httpClient);
        return factory;
    }
}
