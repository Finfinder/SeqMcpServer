using System.Text.Json;
using Seq.Api;
using Seq.Api.Model.Events;
using Seq.Api.Model.Shared;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class QueryLogsToolTests : SdkToolTestBase
{
    protected override string ExpectedErrorSubstring => "Failed to query Seq events:";

    protected override Task<string> InvokeTool(SeqConnection connection, CancellationToken cancellationToken = default) =>
        QueryLogsTool.QueryLogs(connection, cancellationToken: cancellationToken);

    [Fact]
    public async Task QueryLogs_InvalidFromUtcFormat_ReturnsJsonWithError()
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await QueryLogsTool.QueryLogs(connection, fromUtc: "not-a-date");

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to query Seq events:", error);
        Assert.Contains("fromUtc", error);
    }

    [Fact]
    public async Task QueryLogs_InvalidToUtcFormat_ReturnsJsonWithError()
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await QueryLogsTool.QueryLogs(connection, toUtc: "not-a-date");

        using var doc = JsonDocument.Parse(result);
        var error = doc.RootElement.GetProperty("Error").GetString();
        Assert.Contains("Failed to query Seq events:", error);
        Assert.Contains("toUtc", error);
    }

    // --- ProjectToJson tests ---

    [Fact]
    public void ProjectToJson_EmptyList_ReturnsEmptyJsonArray()
    {
        var result = QueryLogsTool.ProjectToJson([]);

        using var doc = JsonDocument.Parse(result);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public void ProjectToJson_SingleEvent_ReturnsCorrectFields()
    {
        var events = new[]
        {
            new EventEntity
            {
                Id = "event-1",
                Timestamp = "2025-01-15T10:00:00.0000000Z",
                Level = "Information",
                RenderedMessage = "Hello world"
            }
        };

        var result = QueryLogsTool.ProjectToJson(events);

        using var doc = JsonDocument.Parse(result);
        var item = doc.RootElement[0];
        Assert.Equal("event-1", item.GetProperty("Id").GetString());
        Assert.Equal("2025-01-15T10:00:00.0000000Z", item.GetProperty("Timestamp").GetString());
        Assert.Equal("Information", item.GetProperty("Level").GetString());
        Assert.Equal("Hello world", item.GetProperty("Message").GetString());
    }

    [Fact]
    public void ProjectToJson_RenderedMessageMappedToMessage()
    {
        var events = new[]
        {
            new EventEntity { RenderedMessage = "Rendered text" }
        };

        var result = QueryLogsTool.ProjectToJson(events);

        using var doc = JsonDocument.Parse(result);
        var item = doc.RootElement[0];
        Assert.Equal("Rendered text", item.GetProperty("Message").GetString());
        Assert.False(item.TryGetProperty("RenderedMessage", out _));
    }

    [Fact]
    public void ProjectToJson_EventWithException_IncludesException()
    {
        var events = new[]
        {
            new EventEntity { Exception = "System.NullReferenceException: Object reference not set." }
        };

        var result = QueryLogsTool.ProjectToJson(events);

        using var doc = JsonDocument.Parse(result);
        var item = doc.RootElement[0];
        Assert.Equal("System.NullReferenceException: Object reference not set.", item.GetProperty("Exception").GetString());
    }

    [Fact]
    public void ProjectToJson_EventWithoutException_OmitsException()
    {
        var events = new[]
        {
            new EventEntity { Id = "e1", Exception = null }
        };

        var result = QueryLogsTool.ProjectToJson(events);

        using var doc = JsonDocument.Parse(result);
        var item = doc.RootElement[0];
        Assert.False(item.TryGetProperty("Exception", out _));
    }

    [Fact]
    public void ProjectToJson_FiltersAtPrefixedProperties()
    {
        var events = new[]
        {
            new EventEntity
            {
                Properties =
                [
                    new EventPropertyPart("@Level", "Information"),
                    new EventPropertyPart("Application", "MyApp"),
                    new EventPropertyPart("@Timestamp", "2025-01-15"),
                    new EventPropertyPart("RequestId", "abc-123")
                ]
            }
        };

        var result = QueryLogsTool.ProjectToJson(events);

        using var doc = JsonDocument.Parse(result);
        var props = doc.RootElement[0].GetProperty("Properties");
        Assert.True(props.TryGetProperty("Application", out _));
        Assert.True(props.TryGetProperty("RequestId", out _));
        Assert.False(props.TryGetProperty("@Level", out _));
        Assert.False(props.TryGetProperty("@Timestamp", out _));
    }

    [Fact]
    public void ProjectToJson_NullProperties_OmitsField()
    {
        var events = new[]
        {
            new EventEntity { Id = "e1", Properties = null }
        };

        var result = QueryLogsTool.ProjectToJson(events);

        using var doc = JsonDocument.Parse(result);
        var item = doc.RootElement[0];
        Assert.False(item.TryGetProperty("Properties", out _));
    }

    [Fact]
    public void ProjectToJson_EmptyProperties_ReturnsEmptyObject()
    {
        var events = new[]
        {
            new EventEntity { Properties = [] }
        };

        var result = QueryLogsTool.ProjectToJson(events);

        using var doc = JsonDocument.Parse(result);
        var props = doc.RootElement[0].GetProperty("Properties");
        Assert.Equal(JsonValueKind.Object, props.ValueKind);
        Assert.Empty(props.EnumerateObject());
    }

    [Fact]
    public void ProjectToJson_MultipleEvents_ReturnsAllInArray()
    {
        var events = new[]
        {
            new EventEntity { Id = "e1", Level = "Information" },
            new EventEntity { Id = "e2", Level = "Warning" },
            new EventEntity { Id = "e3", Level = "Error" }
        };

        var result = QueryLogsTool.ProjectToJson(events);

        using var doc = JsonDocument.Parse(result);
        Assert.Equal(3, doc.RootElement.GetArrayLength());
        Assert.Equal("e1", doc.RootElement[0].GetProperty("Id").GetString());
        Assert.Equal("e2", doc.RootElement[1].GetProperty("Id").GetString());
        Assert.Equal("e3", doc.RootElement[2].GetProperty("Id").GetString());
    }

    // --- Count clamping tests ---

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task QueryLogs_CountBelowMinimum_DoesNotThrowArgumentError(int count)
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await QueryLogsTool.QueryLogs(connection, count: count);

        using var doc = JsonDocument.Parse(result);
        Assert.True(doc.RootElement.TryGetProperty("Error", out var error));
        Assert.DoesNotContain("ArgumentOutOfRange", error.GetString());
    }

    [Theory]
    [InlineData(501)]
    [InlineData(9999)]
    public async Task QueryLogs_CountAboveMaximum_DoesNotThrowArgumentError(int count)
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await QueryLogsTool.QueryLogs(connection, count: count);

        using var doc = JsonDocument.Parse(result);
        Assert.True(doc.RootElement.TryGetProperty("Error", out var error));
        Assert.DoesNotContain("ArgumentOutOfRange", error.GetString());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(250)]
    [InlineData(500)]
    public async Task QueryLogs_CountAtBoundary_DoesNotThrowArgumentError(int count)
    {
        using var connection = new SeqConnection("http://localhost");

        var result = await QueryLogsTool.QueryLogs(connection, count: count);

        using var doc = JsonDocument.Parse(result);
        Assert.True(doc.RootElement.TryGetProperty("Error", out var error));
        Assert.DoesNotContain("ArgumentOutOfRange", error.GetString());
    }
}
