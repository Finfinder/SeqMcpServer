using System.Text.Json;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class VersionToolTests
{
    [Fact]
    public async Task GetVersion_ReturnsJsonWithExpectedFields()
    {
        var json = await VersionTool.GetVersion();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("Name", out _));
        Assert.True(root.TryGetProperty("Version", out _));
        Assert.True(root.TryGetProperty("Runtime", out _));
    }

    [Fact]
    public async Task GetVersion_NameIsSeqMcpServer()
    {
        var json = await VersionTool.GetVersion();
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("seq-mcp-server", doc.RootElement.GetProperty("Name").GetString());
    }

    [Fact]
    public async Task GetVersion_VersionIsNonEmpty()
    {
        var json = await VersionTool.GetVersion();
        using var doc = JsonDocument.Parse(json);

        var version = doc.RootElement.GetProperty("Version").GetString();
        Assert.False(string.IsNullOrEmpty(version));
    }

    [Fact]
    public async Task GetVersion_RuntimeContainsDotNet()
    {
        var json = await VersionTool.GetVersion();
        using var doc = JsonDocument.Parse(json);

        var runtime = doc.RootElement.GetProperty("Runtime").GetString();
        Assert.Contains(".NET", runtime);
    }
}
