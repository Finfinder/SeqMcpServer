using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Seq.Api;

namespace SeqMcpServer.Tools;

[McpServerToolType]
public static class AlertsTool
{
    [McpServerTool(Name = "seq_get_alerts"), Description("Get all configured alerts and their current state from Seq.")]
    public static async Task<string> GetAlerts(
        SeqConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var alertStates = await connection.AlertState.ListAsync();

            // AlertStateEntity has: Id, OwnerId, NotificationLevel, NotificationAppInstanceIds, Activity
            var result = alertStates.Select(a => new
            {
                a.Id,
                a.OwnerId,
                a.NotificationLevel,
                a.NotificationAppInstanceIds,
                a.Activity
            });

            return JsonSerializer.Serialize(result, JsonDefaults.Indented);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = $"Failed to get Seq alert state: {ex.Message}" });
        }
    }
}
