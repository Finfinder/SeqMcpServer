using System.Text.Json;
using Seq.Api;
using Seq.Api.Model.Dashboarding;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class DashboardsToolTests : SdkToolTestBase
{
    protected override string ExpectedErrorSubstring => "Failed to list Seq dashboards:";

    protected override Task<string> InvokeTool(SeqConnection connection, CancellationToken cancellationToken = default) =>
        DashboardsTool.ListDashboards(connection, cancellationToken);

    // --- ProjectToJson tests ---

    [Fact]
    public void ProjectToJson_EmptyList_ReturnsEmptyJsonArray()
    {
        var result = DashboardsTool.ProjectToJson([]);

        using var doc = JsonDocument.Parse(result);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public void ProjectToJson_DashboardWithCharts_ReturnsNestedStructure()
    {
        var dashboards = new[]
        {
            new DashboardEntity
            {
                Id = "dash-1",
                Title = "Overview",
                Charts =
                [
                    new ChartPart
                    {
                        Id = "chart-1",
                        Title = "Error Rate",
                        Queries = [new ChartQueryPart { Where = "@Level = 'Error'", GroupBy = ["Application"] }]
                    }
                ]
            }
        };

        var result = DashboardsTool.ProjectToJson(dashboards);

        using var doc = JsonDocument.Parse(result);
        var dash = doc.RootElement[0];
        Assert.Equal("dash-1", dash.GetProperty("Id").GetString());
        Assert.Equal("Overview", dash.GetProperty("Title").GetString());
        var chart = dash.GetProperty("Charts")[0];
        Assert.Equal("chart-1", chart.GetProperty("Id").GetString());
        Assert.Equal("Error Rate", chart.GetProperty("Title").GetString());
        var query = chart.GetProperty("Queries")[0];
        Assert.Equal("@Level = 'Error'", query.GetProperty("Where").GetString());
        Assert.Equal("Application", query.GetProperty("GroupBy")[0].GetString());
    }

    [Fact]
    public void ProjectToJson_NullCharts_OmitsField()
    {
        var dashboards = new[]
        {
            new DashboardEntity { Id = "dash-1", Title = "Empty", Charts = null }
        };

        var result = DashboardsTool.ProjectToJson(dashboards);

        using var doc = JsonDocument.Parse(result);
        var dash = doc.RootElement[0];
        Assert.False(dash.TryGetProperty("Charts", out _));
    }

    [Fact]
    public void ProjectToJson_ChartWithNullQueries_OmitsField()
    {
        var dashboards = new[]
        {
            new DashboardEntity
            {
                Id = "dash-1",
                Charts = [new ChartPart { Id = "c1", Title = "NoQ", Queries = null }]
            }
        };

        var result = DashboardsTool.ProjectToJson(dashboards);

        using var doc = JsonDocument.Parse(result);
        var chart = doc.RootElement[0].GetProperty("Charts")[0];
        Assert.False(chart.TryGetProperty("Queries", out _));
    }

    [Fact]
    public void ProjectToJson_MultipleDashboards_ReturnsArray()
    {
        var dashboards = new[]
        {
            new DashboardEntity { Id = "d1", Title = "A" },
            new DashboardEntity { Id = "d2", Title = "B" }
        };

        var result = DashboardsTool.ProjectToJson(dashboards);

        using var doc = JsonDocument.Parse(result);
        Assert.Equal(2, doc.RootElement.GetArrayLength());
        Assert.Equal("d1", doc.RootElement[0].GetProperty("Id").GetString());
        Assert.Equal("d2", doc.RootElement[1].GetProperty("Id").GetString());
    }
}
