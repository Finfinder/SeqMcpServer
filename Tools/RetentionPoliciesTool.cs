using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Seq.Api;

namespace SeqMcpServer.Tools;

[McpServerToolType]
public static class RetentionPoliciesTool
{
    [McpServerTool(Name = "seq_get_retention"), Description("Get all retention policies configured in Seq, showing how long log data is kept.")]
    public static async Task<string> GetRetentionPolicies(
        SeqConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var policies = await connection.RetentionPolicies.ListAsync();

            // RetentionPolicyEntity has: RetentionTime (TimeSpan), RemovedSignalExpression (SignalExpressionPart)
            return ProjectToJson(policies);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = $"Failed to get Seq retention policies: {ex.Message}" });
        }
    }

    internal static string ProjectToJson(IEnumerable<Seq.Api.Model.Retention.RetentionPolicyEntity> policies)
    {
        var result = policies.Select(p => new
        {
            p.Id,
            RetentionDays = p.RetentionTime.TotalDays,
            RemovedSignalExpression = p.RemovedSignalExpression?.ToString()
        });

        return JsonSerializer.Serialize(result, JsonDefaults.Indented);
    }
}
