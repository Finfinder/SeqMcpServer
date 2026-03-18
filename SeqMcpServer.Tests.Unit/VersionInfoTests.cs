namespace SeqMcpServer.Tests.Unit;

public class VersionInfoTests
{
    [Fact]
    public void Current_ReturnsNonEmptyString()
    {
        var version = VersionInfo.Current;

        Assert.False(string.IsNullOrEmpty(version));
    }

    [Fact]
    public void Current_StartsWithDigit()
    {
        var version = VersionInfo.Current;

        Assert.True(char.IsDigit(version[0]),
            $"Expected version to start with a digit, but got: '{version}'");
    }
}
