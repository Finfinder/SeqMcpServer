using Microsoft.Extensions.DependencyInjection;
using Seq.Api;

namespace SeqMcpServer;

internal static class HostConfiguration
{
    internal static (string seqUrl, string? seqApiKey) ResolveConfiguration()
    {
        var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341";
        var seqApiKey = Environment.GetEnvironmentVariable("SEQ_API_KEY");
        return (seqUrl, seqApiKey);
    }

    internal static void ValidateSeqUrl(string seqUrl)
    {
        if (!Uri.TryCreate(seqUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException($"Invalid SEQ_URL: '{seqUrl}'");
    }

    internal static void ConfigureServices(IServiceCollection services, string seqUrl, string? seqApiKey)
    {
        services.AddSingleton(sp => new SeqConnection(seqUrl, seqApiKey));

        services.AddHttpClient("Seq", client =>
        {
            client.BaseAddress = new Uri(seqUrl);
            if (!string.IsNullOrEmpty(seqApiKey))
                client.DefaultRequestHeaders.Add("X-Seq-ApiKey", seqApiKey);
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
