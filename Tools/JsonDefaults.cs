using System.Text.Json;
using System.Text.Json.Serialization;

namespace SeqMcpServer.Tools;

internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions Indented = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
