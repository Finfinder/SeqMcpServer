using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Seq.Api;

namespace SeqMcpServer.Tools;

[McpServerToolType]
public static class SignalsTool
{
    [McpServerTool(Name = "seq_list_signals"), Description("List all shared signals (saved log filters/views) defined in Seq.")]
    public static async Task<string> ListSignals(
        SeqConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var signals = await connection.Signals.ListAsync(shared: true);

            // SignalEntity.Filters is List<DescriptiveFilterPart>, each has .Filter (string)
            var result = signals.Select(s => new
            {
                s.Id,
                s.Title,
                s.Description,
                Filters = s.Filters?.Select(f => f.Filter)
            });

            return JsonSerializer.Serialize(result, JsonDefaults.Indented);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = $"Failed to list Seq signals: {ex.Message}" });
        }
    }
}
