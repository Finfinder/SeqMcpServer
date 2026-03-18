using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Seq.Api;

namespace SeqMcpServer.Tools;

[McpServerToolType]
public static class QueryLogsTool
{
    [McpServerTool(Name = "seq_query_logs"), Description("Search Seq log events using a filter expression. Returns matching events with timestamps, levels, messages and properties.")]
    public static async Task<string> QueryLogs(
        SeqConnection connection,
        [Description("Seq filter expression, e.g. '@Level = \"Error\"' or 'Application = \"MyApp\"'. Leave empty for all events.")] string filter = "",
        [Description("Maximum number of events to return (1-500)")] int count = 50,
        [Description("ISO 8601 start time, e.g. '2025-01-15T00:00:00Z'. Defaults to last 24 hours.")] string? fromUtc = null,
        [Description("ISO 8601 end time. Defaults to now.")] string? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            count = Math.Clamp(count, 1, 500);

            var from = string.IsNullOrEmpty(fromUtc)
                ? DateTime.UtcNow.AddHours(-24)
                : DateTime.Parse(fromUtc, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var to = string.IsNullOrEmpty(toUtc)
                ? DateTime.UtcNow
                : DateTime.Parse(toUtc, null, System.Globalization.DateTimeStyles.RoundtripKind);

            cancellationToken.ThrowIfCancellationRequested();

            var events = await connection.Events.ListAsync(
                filter: string.IsNullOrEmpty(filter) ? null : filter,
                count: count,
                render: true,
                fromDateUtc: from,
                toDateUtc: to);

            // EventEntity.Properties is List<EventPropertyPart> with .Name / .Value
            // EventEntity.Timestamp is string (ISO-8601), Level is string
            var result = events.Select(e => new
            {
                e.Id,
                e.Timestamp,
                e.Level,
                Message = e.RenderedMessage,
                Exception = e.Exception,
                Properties = e.Properties?
                    .Where(p => !p.Name.StartsWith("@"))
                    .ToDictionary(p => p.Name, p => p.Value?.ToString())
            });

            return JsonSerializer.Serialize(result, JsonDefaults.Indented);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = $"Failed to query Seq events: {ex.Message}" });
        }
    }
}
