using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Tests.Models;

[TestClass]
public class PolicyEvaluationTests
{
    private const string WorkItemTypeId = "40e92b44-2fe1-4dd6-b3d8-74a9c21d0c6e";

    [TestMethod]
    public void CanRequeue_BuildEnEchec_Vrai()
    {
        var evaluation = Create(PolicyType.BuildTypeId, "Build", "rejected", "Cronus - Build");

        Check.That(evaluation.IsBuild).IsTrue();
        Check.That(evaluation.IsFailed).IsTrue();
        Check.That(evaluation.CanRequeue).IsTrue();
        Check.That(evaluation.Name).IsEqualTo("Cronus - Build");
    }

    [TestMethod]
    public void CanRequeue_BuildCasse_Vrai()
    {
        var evaluation = Create(PolicyType.BuildTypeId.ToUpperInvariant(), "Build", "broken", "Cronus - Build");

        Check.That(evaluation.CanRequeue).IsTrue();
    }

    [TestMethod]
    public void CanRequeue_BuildReussi_Faux()
    {
        var evaluation = Create(PolicyType.BuildTypeId, "Build", "approved", "Cronus - Build");

        Check.That(evaluation.IsFailed).IsFalse();
        Check.That(evaluation.CanRequeue).IsFalse();
    }

    [TestMethod]
    public void CanRequeue_BuildEnCours_Faux()
    {
        var evaluation = Create(PolicyType.BuildTypeId, "Build", "running", "Cronus - Build");

        Check.That(evaluation.CanRequeue).IsFalse();
    }

    [TestMethod]
    public void CanRequeue_PolicyWorkItemEnEchec_Faux_EtNomParDefaut()
    {
        var evaluation = Create(WorkItemTypeId, "Work item linking", "rejected", null);

        Check.That(evaluation.IsFailed).IsTrue();
        Check.That(evaluation.IsBuild).IsFalse();
        Check.That(evaluation.CanRequeue).IsFalse();
        Check.That(evaluation.Name).IsEqualTo("Work item linking");
    }

    [TestMethod]
    public void CanRequeue_PolicyDesactivee_Faux()
    {
        var evaluation = Create(PolicyType.BuildTypeId, "Build", "rejected", "Cronus - Build", isEnabled: false);

        Check.That(evaluation.CanRequeue).IsFalse();
    }

    private static PolicyEvaluation Create(string typeId, string typeName, string status, string? displayName, bool isEnabled = true) =>
        new(Guid.NewGuid(),
            status,
            new PolicyConfiguration(1, new PolicyType(typeId, typeName), isEnabled, true, new PolicySettings(displayName, null)));
}
