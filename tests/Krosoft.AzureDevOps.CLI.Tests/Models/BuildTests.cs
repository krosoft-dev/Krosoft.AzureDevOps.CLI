using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Tests.Models;

[TestClass]
public class BuildTests
{
    [TestMethod]
    public void ShortBranch_RefsHeads_RetourneLeNomCourt()
    {
        Check.That(Create("refs/heads/main", null).ShortBranch).IsEqualTo("main");
        Check.That(Create("refs/heads/renovate/all", null).ShortBranch).IsEqualTo("renovate/all");
    }

    [TestMethod]
    public void ShortBranch_RefsPull_RetireSeulementRefs()
    {
        Check.That(Create("refs/pull/6562/merge", null).ShortBranch).IsEqualTo("pull/6562/merge");
    }

    [TestMethod]
    public void ShortBranch_SansPrefixe_Inchangee()
    {
        Check.That(Create("main", null).ShortBranch).IsEqualTo("main");
    }

    [TestMethod]
    public void PullRequestNumber_DepuisTriggerInfo()
    {
        Check.That(Create("refs/pull/6562/merge", new Dictionary<string, string> { ["pr.number"] = "6562" }).PullRequestNumber).IsEqualTo("6562");
        Check.That(Create("refs/heads/main", new Dictionary<string, string> { ["ci.sourceSha"] = "abc" }).PullRequestNumber).IsNull();
        Check.That(Create("refs/heads/main", null).PullRequestNumber).IsNull();
    }

    private static Build Create(string branch, Dictionary<string, string>? triggerInfo) =>
        new(1,
            "20260902.1",
            "inProgress",
            "pullRequest",
            DateTimeOffset.UtcNow,
            null,
            branch,
            new BuildDefinitionReference(1, "Cronus - Build"),
            new TeamProject(Guid.NewGuid().ToString(), "Cronus"),
            new IdentityRef("Renovate Bot"),
            triggerInfo);
}
