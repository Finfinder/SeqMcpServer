namespace SeqMcpServer.Tools;

internal static class DateRangeHelper
{
    internal static (DateTime from, DateTime to) ParseDateRange(string? fromUtc, string? toUtc)
    {
        var from = string.IsNullOrEmpty(fromUtc)
            ? DateTime.UtcNow.AddHours(-24)
            : DateTime.Parse(fromUtc, null, System.Globalization.DateTimeStyles.RoundtripKind);
        var to = string.IsNullOrEmpty(toUtc)
            ? DateTime.UtcNow
            : DateTime.Parse(toUtc, null, System.Globalization.DateTimeStyles.RoundtripKind);

        return (from, to);
    }
}
