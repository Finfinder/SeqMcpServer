using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Seq.Api;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341";
var seqApiKey = Environment.GetEnvironmentVariable("SEQ_API_KEY");

builder.Services.AddSingleton(new SeqConnection(seqUrl, seqApiKey));

if (!Uri.TryCreate(seqUrl, UriKind.Absolute, out _))
    throw new InvalidOperationException($"Invalid SEQ_URL: '{seqUrl}'");

Console.Error.WriteLine(string.IsNullOrEmpty(seqApiKey)
    ? "WARNING: No SEQ_API_KEY configured \u2014 connecting without authentication"
    : "Seq MCP server starting with API key authentication");

builder.Services.AddHttpClient("Seq", client =>
{
    client.BaseAddress = new Uri(seqUrl);
    if (!string.IsNullOrEmpty(seqApiKey))
        client.DefaultRequestHeaders.Add("X-Seq-ApiKey", seqApiKey);
});

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new()
        {
            Name = "seq-mcp-server",
            Version = "1.0.0"
        };
    })
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
