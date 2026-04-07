namespace SeqMcpServer.Tools;

internal static class DateRangeHelper
{
    internal static (DateTime from, DateTime to) ParseDateRange(string? fromUtc, string? toUtc)
    {
        var from = string.IsNullOrEmpty(fromUtc)
            ? DateTime.UtcNow.AddHours(-24)
            : ParseIso8601(fromUtc, nameof(fromUtc));
        var to = string.IsNullOrEmpty(toUtc)
            ? DateTime.UtcNow
            : ParseIso8601(toUtc, nameof(toUtc));

        return (from, to);
    }

    private static DateTime ParseIso8601(string value, string parameterName)
    {
        try
        {
            return DateTime.Parse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
        }
        catch (FormatException)
        {
            throw new ArgumentException(
                $"Invalid {parameterName} date format: '{value}'. Expected ISO 8601.",
                parameterName);
        }
    }
}
