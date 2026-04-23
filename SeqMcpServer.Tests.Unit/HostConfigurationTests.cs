using Microsoft.Extensions.DependencyInjection;
using Seq.Api;

namespace SeqMcpServer.Tests.Unit;

public class HostConfigurationTests
{
    // ValidateSeqUrl

    [Fact]
    public void ValidateSeqUrl_ValidHttpUrl_DoesNotThrow()
    {
        HostConfiguration.ValidateSeqUrl("http://localhost:5341");
    }

    [Fact]
    public void ValidateSeqUrl_ValidHttpsUrl_DoesNotThrow()
    {
        HostConfiguration.ValidateSeqUrl("https://seq.example.com");
    }

    [Fact]
    public void ValidateSeqUrl_EmptyString_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            HostConfiguration.ValidateSeqUrl(""));
    }

    [Fact]
    public void ValidateSeqUrl_RelativeUrl_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            HostConfiguration.ValidateSeqUrl("/relative/path"));
    }

    [Fact]
    public void ValidateSeqUrl_RandomText_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            HostConfiguration.ValidateSeqUrl("not-a-url"));
    }

    [Fact]
    public void ValidateSeqUrl_InvalidUrl_ErrorMessageContainsUrlAndPrefix()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            HostConfiguration.ValidateSeqUrl("invalid"));

        Assert.Contains("Invalid SEQ_URL", ex.Message);
        Assert.Contains("invalid", ex.Message);
    }

    // ConfigureServices — SeqConnection

    [Fact]
    public void ConfigureServices_SeqConnection_IsRegistered()
    {
        var services = new ServiceCollection();

        HostConfiguration.ConfigureServices(services, "http://localhost:5341", null);

        var provider = services.BuildServiceProvider();
        var connection = provider.GetService<SeqConnection>();
        Assert.NotNull(connection);
    }

    [Fact]
    public void ConfigureServices_SeqConnection_IsSingleton()
    {
        var services = new ServiceCollection();

        HostConfiguration.ConfigureServices(services, "http://localhost:5341", null);

        var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<SeqConnection>();
        var second = provider.GetRequiredService<SeqConnection>();
        Assert.True(ReferenceEquals(first, second));
    }

    // ConfigureServices — HttpClient "Seq"

    [Fact]
    public void ConfigureServices_HttpClient_HasCorrectBaseAddress()
    {
        var services = new ServiceCollection();

        HostConfiguration.ConfigureServices(services, "http://test:5341", null);

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("Seq");
        Assert.Equal(new Uri("http://test:5341"), client.BaseAddress);
    }

    [Fact]
    public void ConfigureServices_HttpClientWithApiKey_HasSeqApiKeyHeader()
    {
        var services = new ServiceCollection();

        HostConfiguration.ConfigureServices(services, "http://localhost:5341", "my-key");

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("Seq");
        Assert.True(client.DefaultRequestHeaders.Contains("X-Seq-ApiKey"));
        Assert.Equal("my-key", client.DefaultRequestHeaders.GetValues("X-Seq-ApiKey").Single());
    }

    [Fact]
    public void ConfigureServices_HttpClientWithoutApiKey_NoSeqApiKeyHeader()
    {
        var services = new ServiceCollection();

        HostConfiguration.ConfigureServices(services, "http://localhost:5341", null);

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("Seq");
        Assert.False(client.DefaultRequestHeaders.Contains("X-Seq-ApiKey"));
    }
}
