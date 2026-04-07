namespace SeqMcpServer.Tests.Integration.Helpers;

internal sealed class TestHttpClientFactory : IHttpClientFactory, IDisposable
{
    private readonly string _baseAddress;
    private HttpClient? _httpClient;

    public TestHttpClientFactory(string baseAddress)
    {
        _baseAddress = baseAddress;
    }

    public HttpClient CreateClient(string name)
    {
        _httpClient ??= new HttpClient { BaseAddress = new Uri(_baseAddress) };
        return _httpClient;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
