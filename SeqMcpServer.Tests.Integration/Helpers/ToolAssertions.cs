using System.Text.Json;

namespace SeqMcpServer.Tests.Integration.Helpers;

internal static class ToolAssertions
{
    internal static void AssertNoToolError(string json)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.False(
            doc.ValueKind == JsonValueKind.Object && doc.TryGetProperty("Error", out _),
            "Expected no Error in response");
    }
}
