using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Seq.Api;

namespace SeqMcpServer.Tools;

[McpServerToolType]
public static class DashboardsTool
{
    [McpServerTool(Name = "seq_list_dashboards"), Description("List all shared dashboards configured in Seq with their chart definitions.")]
    public static async Task<string> ListDashboards(
        SeqConnection connection,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dashboards = await connection.Dashboards.ListAsync(shared: true);

            // DashboardEntity has: Id, Title, OwnerId, IsProtected, SignalExpression, Charts
            // ChartPart has: Id, Title, Description, Queries (List<ChartQueryPart>), SignalExpression
            // ChartQueryPart has: Where, Measurements, GroupBy, Having, OrderBy, Limit
            var result = dashboards.Select(d => new
            {
                d.Id,
                d.Title,
                Charts = d.Charts?.Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    Queries = c.Queries?.Select(q => new
                    {
                        q.Where,
                        q.GroupBy
                    })
                })
            });

            return JsonSerializer.Serialize(result, JsonDefaults.Indented);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = $"Failed to list Seq dashboards: {ex.Message}" });
        }
    }
}
