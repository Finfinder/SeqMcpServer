using System.Text.Json;
using Seq.Api;
using Seq.Api.Model.Shared;
using Seq.Api.Model.Signals;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class SignalsToolTests : SdkToolTestBase
{
    protected override string ExpectedErrorSubstring => "Failed to list Seq signals:";

    protected override Task<string> InvokeTool(SeqConnection connection, CancellationToken cancellationToken = default) =>
        SignalsTool.ListSignals(connection, cancellationToken);

    // --- ProjectToJson tests ---

    [Fact]
    public void ProjectToJson_EmptyList_ReturnsEmptyJsonArray()
    {
        var result = SignalsTool.ProjectToJson([]);

        using var doc = JsonDocument.Parse(result);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public void ProjectToJson_SignalWithAllFields_ReturnsComplete()
    {
        var signals = new[]
        {
            new SignalEntity
            {
                Id = "signal-1",
                Title = "Errors",
                Description = "All error events",
                Filters = [new DescriptiveFilterPart { Filter = "@Level = 'Error'" }]
            }
        };

        var result = SignalsTool.ProjectToJson(signals);

        using var doc = JsonDocument.Parse(result);
        var item = doc.RootElement[0];
        Assert.Equal("signal-1", item.GetProperty("Id").GetString());
        Assert.Equal("Errors", item.GetProperty("Title").GetString());
        Assert.Equal("All error events", item.GetProperty("Description").GetString());
        Assert.Equal(1, item.GetProperty("Filters").GetArrayLength());
        Assert.Equal("@Level = 'Error'", item.GetProperty("Filters")[0].GetString());
    }

    [Fact]
    public void ProjectToJson_NullDescription_OmitsField()
    {
        var signals = new[]
        {
            new SignalEntity { Id = "s1", Title = "MySignal", Description = null }
        };

        var result = SignalsTool.ProjectToJson(signals);

        using var doc = JsonDocument.Parse(result);
        var item = doc.RootElement[0];
        Assert.False(item.TryGetProperty("Description", out _));
    }

    [Fact]
    public void ProjectToJson_NullFilters_OmitsField()
    {
        var signals = new[]
        {
            new SignalEntity { Id = "s1", Title = "MySignal", Filters = null }
        };

        var result = SignalsTool.ProjectToJson(signals);

        using var doc = JsonDocument.Parse(result);
        var item = doc.RootElement[0];
        Assert.False(item.TryGetProperty("Filters", out _));
    }

    [Fact]
    public void ProjectToJson_FiltersExtracted_ReturnsFilterStrings()
    {
        var signals = new[]
        {
            new SignalEntity
            {
                Id = "s1",
                Filters =
                [
                    new DescriptiveFilterPart { Filter = "@Level = 'Error'" },
                    new DescriptiveFilterPart { Filter = "Application = 'Api'" }
                ]
            }
        };

        var result = SignalsTool.ProjectToJson(signals);

        using var doc = JsonDocument.Parse(result);
        var filters = doc.RootElement[0].GetProperty("Filters");
        Assert.Equal(2, filters.GetArrayLength());
        Assert.Equal("@Level = 'Error'", filters[0].GetString());
        Assert.Equal("Application = 'Api'", filters[1].GetString());
    }

    [Fact]
    public void ProjectToJson_MultipleSignals_ReturnsArray()
    {
        var signals = new[]
        {
            new SignalEntity { Id = "s1", Title = "Errors" },
            new SignalEntity { Id = "s2", Title = "Warnings" }
        };

        var result = SignalsTool.ProjectToJson(signals);

        using var doc = JsonDocument.Parse(result);
        Assert.Equal(2, doc.RootElement.GetArrayLength());
        Assert.Equal("s1", doc.RootElement[0].GetProperty("Id").GetString());
        Assert.Equal("s2", doc.RootElement[1].GetProperty("Id").GetString());
    }
}
