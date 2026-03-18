using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace SeqMcpServer.Tools;

[McpServerToolType]
public static class AlertsTool
{
    [McpServerTool(Name = "seq_get_alerts"), Description("Get all configured alerts and their current state from Seq.")]
    public static async Task<string> GetAlerts(
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient("Seq");
            var response = await httpClient.GetAsync("/api/alertstate", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = $"Failed to get Seq alert state: {ex.Message}" });
        }
    }
}
