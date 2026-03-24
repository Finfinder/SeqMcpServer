using SeqMcpServer.Tools;

namespace SeqMcpServer.Tests.Unit;

public class DateRangeHelperTests
{
    [Fact]
    public void ParseDateRange_BothNull_ReturnsDefaultRange()
    {
        var before = DateTime.UtcNow;

        var (from, to) = DateRangeHelper.ParseDateRange(null, null);

        var after = DateTime.UtcNow;
        Assert.InRange(to, before, after);
        Assert.InRange(from, before.AddHours(-24), after.AddHours(-24));
    }

    [Fact]
    public void ParseDateRange_BothEmpty_ReturnsDefaultRange()
    {
        var before = DateTime.UtcNow;

        var (from, to) = DateRangeHelper.ParseDateRange("", "");

        var after = DateTime.UtcNow;
        Assert.InRange(to, before, after);
        Assert.InRange(from, before.AddHours(-24), after.AddHours(-24));
    }

    [Fact]
    public void ParseDateRange_BothValidIso8601_ReturnsExactValues()
    {
        var (from, to) = DateRangeHelper.ParseDateRange(
            "2025-01-15T00:00:00Z",
            "2025-01-16T00:00:00Z");

        Assert.Equal(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc), from);
        Assert.Equal(new DateTime(2025, 1, 16, 0, 0, 0, DateTimeKind.Utc), to);
    }

    [Fact]
    public void ParseDateRange_InvalidFromUtc_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() =>
            DateRangeHelper.ParseDateRange("not-a-date", null));
    }

    [Fact]
    public void ParseDateRange_InvalidToUtc_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() =>
            DateRangeHelper.ParseDateRange(null, "not-a-date"));
    }

    [Fact]
    public void ParseDateRange_FromProvidedToNull_ReturnsMixedValues()
    {
        var before = DateTime.UtcNow;

        var (from, to) = DateRangeHelper.ParseDateRange("2025-06-01T12:00:00Z", null);

        var after = DateTime.UtcNow;
        Assert.Equal(new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc), from);
        Assert.InRange(to, before, after);
    }
}
