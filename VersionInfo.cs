using System.Reflection;

namespace SeqMcpServer;

internal static class VersionInfo
{
    public static string Current { get; } =
        Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "0.0.0";
}
