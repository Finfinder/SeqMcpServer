namespace SeqMcpServer.Tests.Unit;

public class ReleaseWorkflowContractTests
{
    [Fact]
    public void ReleaseWorkflow_UsesLocalReleaseAdapters()
    {
        var workflowPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.github/workflows/release.yml"));

        Assert.True(File.Exists(workflowPath), $"Missing release workflow: {workflowPath}");

        var workflowText = File.ReadAllText(workflowPath);

        Assert.Contains("./.github/workflows/reusable-version-consistency.yml", workflowText);
        Assert.Contains("./.github/workflows/reusable-next-version-request.yml", workflowText);
        Assert.Contains("source-repository: ${{ github.repository }}", workflowText);
        Assert.Contains("repository-ref: ${{ github.ref }}", workflowText);
        Assert.Contains("expected-release-version: ${{ github.ref_name }}", workflowText);
        Assert.Contains("next-version-request, build-self-contained, build-framework-dependent, pack-nuget", workflowText);
        Assert.DoesNotContain("Validate next version request", workflowText);
    }
}
