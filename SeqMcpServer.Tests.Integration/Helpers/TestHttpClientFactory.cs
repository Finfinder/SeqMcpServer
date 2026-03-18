namespace SeqMcpServer.Tests.Integration.Helpers;

internal class TestHttpClientFactory : IHttpClientFactory
{
    private readonly string _baseAddress;

    public TestHttpClientFactory(string baseAddress)
    {
        _baseAddress = baseAddress;
    }

    public HttpClient CreateClient(string name)
    {
        return new HttpClient { BaseAddress = new Uri(_baseAddress) };
    }
}
