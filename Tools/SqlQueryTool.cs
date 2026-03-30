using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using ModelContextProtocol.Server;
using Seq.Api;

namespace SeqMcpServer.Tools;

[McpServerToolType]
public static class SqlQueryTool
{
    [McpServerTool(Name = "seq_run_sql"), Description("Execute a SQL query against Seq log data. Use standard Seq SQL syntax, e.g. 'select count(*) from stream group by @Level'.")]
    public static async Task<string> RunSql(
        SeqConnection connection,
        [Description("SQL query to execute against Seq event stream")] string query,
        [Description("ISO 8601 range start. Defaults to last 24h.")] string? fromUtc = null,
        [Description("ISO 8601 range end. Defaults to now.")] string? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (from, to) = DateRangeHelper.ParseDateRange(fromUtc, toUtc);

            // Guard: append LIMIT if query doesn't contain one
            if (!HasLimitClause(query))
                query += " limit 1000";

            cancellationToken.ThrowIfCancellationRequested();

            var result = await connection.Data.QueryAsync(query, rangeStartUtc: from, rangeEndUtc: to);

            var output = new { result.Columns, result.Rows };
            return JsonSerializer.Serialize(output, JsonDefaults.Indented);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { Error = $"Failed to execute Seq SQL query: {ex.Message}" });
        }
    }

    internal static bool HasLimitClause(string query) =>
        Regex.IsMatch(query, @"\bLIMIT\s+\d+", RegexOptions.IgnoreCase);
}
