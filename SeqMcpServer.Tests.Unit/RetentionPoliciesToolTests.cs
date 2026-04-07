using System.Text.Json;
using Seq.Api;
using Seq.Api.Model.Retention;
using SeqMcpServer.Tests.Unit.Helpers;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class RetentionPoliciesToolTests : SdkToolTestBase
{
    protected override string ExpectedErrorSubstring => "Failed to get Seq retention policies:";

    protected override Task<string> InvokeTool(SeqConnection connection, CancellationToken cancellationToken = default) =>
        RetentionPoliciesTool.GetRetentionPolicies(connection, cancellationToken);

    // --- ProjectToJson tests ---

    [Fact]
    public void ProjectToJson_EmptyList_ReturnsEmptyJsonArray()
    {
        var result = RetentionPoliciesTool.ProjectToJson([]);

        using var doc = JsonDocument.Parse(result);
        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public void ProjectToJson_PolicyWith30Days_ReturnsCorrectDays()
    {
        var policies = new[]
        {
            new RetentionPolicyEntity
            {
                Id = "rp-1",
                RetentionTime = TimeSpan.FromDays(30),
                RemovedSignalExpression = null
            }
        };

        var result = RetentionPoliciesTool.ProjectToJson(policies);

        using var doc = JsonDocument.Parse(result);
        var item = doc.RootElement[0];
        Assert.Equal("rp-1", item.GetProperty("Id").GetString());
        Assert.Equal(30.0, item.GetProperty("RetentionDays").GetDouble());
    }

    [Fact]
    public void ProjectToJson_PolicyWithFractionalDays_ReturnsPrecise()
    {
        var policies = new[]
        {
            new RetentionPolicyEntity
            {
                Id = "rp-2",
                RetentionTime = TimeSpan.FromHours(36),
                RemovedSignalExpression = null
            }
        };

        var result = RetentionPoliciesTool.ProjectToJson(policies);

        using var doc = JsonDocument.Parse(result);
        Assert.Equal(1.5, doc.RootElement[0].GetProperty("RetentionDays").GetDouble());
    }

    [Fact]
    public void ProjectToJson_NullSignalExpression_OmitsField()
    {
        var policies = new[]
        {
            new RetentionPolicyEntity
            {
                Id = "rp-3",
                RetentionTime = TimeSpan.FromDays(7),
                RemovedSignalExpression = null
            }
        };

        var result = RetentionPoliciesTool.ProjectToJson(policies);

        using var doc = JsonDocument.Parse(result);
        Assert.False(doc.RootElement[0].TryGetProperty("RemovedSignalExpression", out _));
    }

    [Fact]
    public void ProjectToJson_ZeroRetention_ReturnsZero()
    {
        var policies = new[]
        {
            new RetentionPolicyEntity { Id = "rp-4", RetentionTime = TimeSpan.Zero, RemovedSignalExpression = null }
        };

        var result = RetentionPoliciesTool.ProjectToJson(policies);

        using var doc = JsonDocument.Parse(result);
        Assert.Equal(0.0, doc.RootElement[0].GetProperty("RetentionDays").GetDouble());
    }

    [Fact]
    public void ProjectToJson_MultiplePolicies_ReturnsArray()
    {
        var policies = new[]
        {
            new RetentionPolicyEntity { Id = "rp-1", RetentionTime = TimeSpan.FromDays(7), RemovedSignalExpression = null },
            new RetentionPolicyEntity { Id = "rp-2", RetentionTime = TimeSpan.FromDays(30), RemovedSignalExpression = null }
        };

        var result = RetentionPoliciesTool.ProjectToJson(policies);

        using var doc = JsonDocument.Parse(result);
        Assert.Equal(2, doc.RootElement.GetArrayLength());
        Assert.Equal("rp-1", doc.RootElement[0].GetProperty("Id").GetString());
        Assert.Equal("rp-2", doc.RootElement[1].GetProperty("Id").GetString());
    }
}

