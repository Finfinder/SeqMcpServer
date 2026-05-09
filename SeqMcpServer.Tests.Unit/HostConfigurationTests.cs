using Microsoft.Extensions.DependencyInjection;
using Seq.Api;

namespace SeqMcpServer.Tests.Unit;

public class HostConfigurationTests
{
    private static IDisposable OverrideEnvironmentVariables(string? seqUrl, string? seqApiKey)
    {
        return new EnvironmentVariableScope(
            ("SEQ_URL", seqUrl),
            ("SEQ_API_KEY", seqApiKey));
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly (string Name, string? Value)[] _originalValues;

        public EnvironmentVariableScope(params (string Name, string? Value)[] variables)
        {
            _originalValues = variables
                .Select(variable => (variable.Name, Environment.GetEnvironmentVariable(variable.Name)))
                .ToArray();

            foreach (var variable in variables)
            {
                Environment.SetEnvironmentVariable(variable.Name, variable.Value);
            }
        }

        public void Dispose()
        {
            foreach (var originalValue in _originalValues)
            {
                Environment.SetEnvironmentVariable(originalValue.Name, originalValue.Value);
            }
        }
    }

    // ResolveConfiguration

    [Fact]
    public void ResolveConfiguration_WithoutEnvironmentVariables_ReturnsDefaultSeqUrl()
    {
        using var _ = OverrideEnvironmentVariables(null, null);

        var configuration = HostConfiguration.ResolveConfiguration();

        Assert.Equal("http://localhost:5341", configuration.SeqUrl);
        Assert.Null(configuration.SeqApiKey);
    }

    [Fact]
    public void ResolveConfiguration_WithSeqUrlEnvironmentVariable_ReturnsConfiguredSeqUrl()
    {
        using var _ = OverrideEnvironmentVariables("https://seq.example.com", null);

        var configuration = HostConfiguration.ResolveConfiguration();

        Assert.Equal("https://seq.example.com", configuration.SeqUrl);
    }

    [Fact]
    public void ResolveConfiguration_WithSeqApiKeyEnvironmentVariable_ReturnsConfiguredSeqApiKey()
    {
        using var _ = OverrideEnvironmentVariables(null, "test-api-key");

        var configuration = HostConfiguration.ResolveConfiguration();

        Assert.Equal("test-api-key", configuration.SeqApiKey);
    }

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
    public void ValidateSeqUrl_UrlWithSpaces_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            HostConfiguration.ValidateSeqUrl("not a valid url"));
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

        HostConfiguration.ConfigureServices(services, new SeqHostConfiguration("http://localhost:5341", null));

        var provider = services.BuildServiceProvider();
        var connection = provider.GetService<SeqConnection>();
        Assert.NotNull(connection);
    }

    [Fact]
    public void ConfigureServices_SeqConnection_IsSingleton()
    {
        var services = new ServiceCollection();

        HostConfiguration.ConfigureServices(services, new SeqHostConfiguration("http://localhost:5341", null));

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

        HostConfiguration.ConfigureServices(services, new SeqHostConfiguration("http://test:5341", null));

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("Seq");
        Assert.Equal(new Uri("http://test:5341"), client.BaseAddress);
    }

    [Fact]
    public void ConfigureServices_HttpClientWithApiKey_HasSeqApiKeyHeader()
    {
        var services = new ServiceCollection();

        HostConfiguration.ConfigureServices(services, new SeqHostConfiguration("http://localhost:5341", "my-key"));

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

        HostConfiguration.ConfigureServices(services, new SeqHostConfiguration("http://localhost:5341", null));

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("Seq");
        Assert.False(client.DefaultRequestHeaders.Contains("X-Seq-ApiKey"));
    }
}
