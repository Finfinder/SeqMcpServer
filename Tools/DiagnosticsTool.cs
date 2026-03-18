using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace SeqMcpServer.Tools;

[McpServerToolType]
public static class DiagnosticsTool
{
    [McpServerTool(Name = "seq_get_diagnostics"), Description("Get Seq server diagnostics including ingestion status, storage usage and system metrics.")]
    public static async Task<string> GetDiagnostics(
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient("Seq");
            var response = await httpClient.GetAsync("/api/diagnostics/report", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = $"Failed to get Seq diagnostics: {ex.Message}" });
        }
    }
}
