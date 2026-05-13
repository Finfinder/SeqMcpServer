namespace SeqMcpServer.Tests.Unit;

public class ReleaseWorkflowContractTests
{
    private static string ReadRepositoryFile(string relativePath)
    {
        var absolutePath = GetRepositoryPath(relativePath);

        Assert.True(File.Exists(absolutePath), $"Missing repository file: {absolutePath}");

        return File.ReadAllText(absolutePath);
    }

    private static void AssertRepositoryFileExists(string relativePath)
    {
        var absolutePath = GetRepositoryPath(relativePath);

        Assert.True(File.Exists(absolutePath), $"Missing repository file: {absolutePath}");
    }

    private static string GetRepositoryPath(string relativePath)
    {
        var normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../", normalizedRelativePath));
    }

    [Fact]
    public void ReleaseWorkflow_UsesLocalReleaseAdapters()
    {
        var workflowText = ReadRepositoryFile(".github/workflows/release.yml");

        Assert.Contains("./.github/workflows/reusable-version-consistency.yml", workflowText);
        Assert.Contains("./.github/workflows/reusable-next-version-request.yml", workflowText);
        Assert.Contains("softprops/action-gh-release@b4309332981a82ec1c5618f44dd2e27cc8bfbfda", workflowText);
        Assert.Contains("source-repository: ${{ github.repository }}", workflowText);
        Assert.Contains("repository-ref: ${{ github.ref }}", workflowText);
        Assert.Contains("expected-release-version: ${{ github.ref_name }}", workflowText);
        Assert.Contains("next-version-request, build-self-contained, build-framework-dependent, pack-nuget", workflowText);
        Assert.DoesNotContain("Finfinder/AI_Instruction", workflowText);
        Assert.DoesNotContain("softprops/action-gh-release@v2", workflowText);
        Assert.DoesNotContain("Validate next version request", workflowText);
    }

    [Fact]
    public void OpenNextVersionBranchWorkflow_UsesLocalReleaseAdapter()
    {
        var workflowText = ReadRepositoryFile(".github/workflows/open-next-version-branch.yml");

        Assert.Contains("./.github/workflows/reusable-open-next-version-branch.yml", workflowText);
        Assert.Contains("workflow-run-id: ${{ github.event.workflow_run.id }}", workflowText);
        Assert.Contains("artifact-name: next-version-request", workflowText);
        Assert.DoesNotContain("Finfinder/AI_Instruction", workflowText);
        Assert.DoesNotContain("secrets: inherit", workflowText);
    }

    [Fact]
    public void LocalReleaseAutomationAssets_ArePresent()
    {
        AssertRepositoryFileExists(".github/workflows/reusable-version-consistency.yml");
        AssertRepositoryFileExists(".github/workflows/reusable-next-version-request.yml");
        AssertRepositoryFileExists(".github/workflows/reusable-open-next-version-branch.yml");
        AssertRepositoryFileExists("scripts/version-target-strategies.ps1");
        AssertRepositoryFileExists("scripts/next-version-manifest.ps1");
        AssertRepositoryFileExists("scripts/validate-version-consistency.ps1");
        AssertRepositoryFileExists("scripts/validate-next-version-request.ps1");
        AssertRepositoryFileExists("scripts/open-next-version-branch.ps1");
    }

    [Fact]
    public void ThirdPartyActionPinningWorkflow_UsesLocalPolicyBundle()
    {
        var wrapperText = ReadRepositoryFile(".github/workflows/third-party-action-pinning.yml");
        var reusableText = ReadRepositoryFile(".github/workflows/reusable-third-party-action-pinning.yml");

        Assert.Contains("./.github/workflows/reusable-third-party-action-pinning.yml", wrapperText);
        Assert.Contains("Join-Path $repositoryRoot '.github/actions-security/zizmor.yml'", reusableText);
        Assert.Contains("Policy source: repo-local mirror", reusableText);
        Assert.DoesNotContain("automation-repository:", reusableText);
        Assert.DoesNotContain("Join-Path $env:RUNNER_TEMP 'zizmor-third-party-action-pinning.yml'", reusableText);
        AssertRepositoryFileExists(".github/actions-security/zizmor.yml");
    }
}
