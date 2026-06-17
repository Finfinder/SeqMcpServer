using System.Text.Json;

namespace SeqMcpServer.Tests.Unit.Helpers;

internal static class ToolAssertions
{
    internal static string AssertJsonError(string result, string expectedSubstring)
    {
        using var doc = JsonDocument.Parse(result);

        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
        Assert.True(doc.RootElement.TryGetProperty("Error", out var errorElement), "Expected JSON response to contain 'Error' property.");
        Assert.Equal(JsonValueKind.String, errorElement.ValueKind);

        var error = errorElement.GetString();
        Assert.False(string.IsNullOrWhiteSpace(error), "Expected 'Error' property to contain a non-empty message.");
        Assert.Contains(expectedSubstring, error);

        return error!;
    }

    internal static void AssertNoToolError(string json)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.False(
            doc.ValueKind == JsonValueKind.Object && doc.TryGetProperty("Error", out _),
            "Expected no Error in response");
    }
}
