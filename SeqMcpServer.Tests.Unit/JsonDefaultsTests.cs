using System.Text.Json;
using System.Text.Json.Serialization;
using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class JsonDefaultsTests
{
    [Fact]
    public void Indented_WriteIndented_IsTrue()
    {
        Assert.True(JsonDefaults.Indented.WriteIndented);
    }

    [Fact]
    public void Indented_DefaultIgnoreCondition_IsWhenWritingNull()
    {
        Assert.Equal(JsonIgnoreCondition.WhenWritingNull, JsonDefaults.Indented.DefaultIgnoreCondition);
    }

    [Fact]
    public void Indented_SerializesWithIndentation_AndOmitsNulls()
    {
        var obj = new { Name = "test", Value = (string?)null, Count = 1 };
        var json = JsonSerializer.Serialize(obj, JsonDefaults.Indented);

        Assert.Contains("\"Name\"", json);
        Assert.Contains("\"Count\"", json);
        Assert.DoesNotContain("\"Value\"", json);
        Assert.Contains("\n", json);
    }
}
