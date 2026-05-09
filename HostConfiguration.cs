using Microsoft.Extensions.DependencyInjection;
using Seq.Api;

namespace SeqMcpServer;

internal sealed record SeqHostConfiguration(string SeqUrl, string? SeqApiKey);

internal static class HostConfiguration
{
    internal static SeqHostConfiguration ResolveConfiguration()
    {
        var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341";
        var seqApiKey = Environment.GetEnvironmentVariable("SEQ_API_KEY");
        return new SeqHostConfiguration(seqUrl, seqApiKey);
    }

    internal static void ValidateSeqUrl(string seqUrl)
    {
        if (!Uri.TryCreate(seqUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException($"Invalid SEQ_URL: '{seqUrl}'");
    }

    internal static void ConfigureServices(IServiceCollection services, SeqHostConfiguration configuration)
    {
        services.AddSingleton(sp => new SeqConnection(configuration.SeqUrl, configuration.SeqApiKey));

        services.AddHttpClient("Seq", client =>
        {
            client.BaseAddress = new Uri(configuration.SeqUrl);
            if (!string.IsNullOrEmpty(configuration.SeqApiKey))
                client.DefaultRequestHeaders.Add("X-Seq-ApiKey", configuration.SeqApiKey);
        });

        services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new()
                {
                    Name = "seq-mcp-server",
                    Version = VersionInfo.Current
                };
            })
            .WithStdioServerTransport()
            .WithToolsFromAssembly();
    }
}
