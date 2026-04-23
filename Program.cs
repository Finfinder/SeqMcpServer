using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SeqMcpServer;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var (seqUrl, seqApiKey) = HostConfiguration.ResolveConfiguration();
HostConfiguration.ValidateSeqUrl(seqUrl);

Console.Error.WriteLine(string.IsNullOrEmpty(seqApiKey)
    ? $"WARNING: Seq MCP server v{VersionInfo.Current} \u2014 no SEQ_API_KEY configured, connecting without authentication"
    : $"Seq MCP server v{VersionInfo.Current} starting with API key authentication");

HostConfiguration.ConfigureServices(builder.Services, seqUrl, seqApiKey);

await builder.Build().RunAsync();
