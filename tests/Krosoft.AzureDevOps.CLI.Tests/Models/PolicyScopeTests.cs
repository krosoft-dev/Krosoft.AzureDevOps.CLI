using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Tests.Models;

[TestClass]
public class PolicyScopeTests
{
    [TestMethod]
    public void BranchLabel_Exact_RetireRefsHeads()
    {
        var scope = new PolicyScope("95d2a288-14c3-4034-8b6e-97454e9a2327", "refs/heads/main", "Exact");

        Check.That(scope.IsProjectWide).IsFalse();
        Check.That(scope.BranchLabel).IsEqualTo("main");
    }

    [TestMethod]
    public void BranchLabel_Prefix_AjouteJoker()
    {
        var scope = new PolicyScope(null, "refs/heads/release/", "Prefix");

        Check.That(scope.BranchLabel).IsEqualTo("release/*");
    }

    [TestMethod]
    public void BranchLabel_DefaultBranch()
    {
        var scope = new PolicyScope(null, null, "DefaultBranch");

        Check.That(scope.IsProjectWide).IsTrue();
        Check.That(scope.BranchLabel).IsEqualTo("(branche par défaut)");
    }

    [TestMethod]
    public void BranchLabel_SansRef_Toutes()
    {
        var scope = new PolicyScope(null, null, null);

        Check.That(scope.BranchLabel).IsEqualTo("(toutes)");
    }
}
