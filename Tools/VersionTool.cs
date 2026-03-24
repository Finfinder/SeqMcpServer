using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace SeqMcpServer.Tools;

[McpServerToolType]
public static class VersionTool
{
    [McpServerTool(Name = "seq_get_version"), Description("Get the MCP server version, name and runtime information.")]
    public static Task<string> GetVersion(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = new
            {
                Name = "seq-mcp-server",
                Version = VersionInfo.Current,
                Runtime = RuntimeInformation.FrameworkDescription
            };

            return Task.FromResult(JsonSerializer.Serialize(result, JsonDefaults.Indented));
        }
        catch (Exception ex)
        {
            return Task.FromResult(JsonSerializer.Serialize(new { Error = $"Failed to get server version: {ex.Message}" }));
        }
    }
}
