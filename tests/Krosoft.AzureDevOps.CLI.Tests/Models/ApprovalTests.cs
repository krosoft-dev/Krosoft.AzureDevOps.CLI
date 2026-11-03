using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Tests.Models;

[TestClass]
public class ApprovalTests
{
    [TestMethod]
    public void PendingApprovers_GardeSeulementLesEtapesEnAttente()
    {
        var approval = Create([
            new ApprovalStep(new IdentityRef("Alice"), "pending"),
            new ApprovalStep(new IdentityRef("Bob"), "approved"),
            new ApprovalStep(new IdentityRef("Chloé"), "Pending")
        ]);

        Check.That(approval.PendingApprovers).IsEqualTo("Alice, Chloé");
    }

    [TestMethod]
    public void PendingApprovers_SansDoublon()
    {
        var approval = Create([
            new ApprovalStep(new IdentityRef("Alice"), "pending"),
            new ApprovalStep(new IdentityRef("alice"), "pending")
        ]);

        Check.That(approval.PendingApprovers).IsEqualTo("Alice");
    }

    [TestMethod]
    public void PendingApprovers_AucuneEtape_RetourneTiret()
    {
        Check.That(Create(null).PendingApprovers).IsEqualTo("-");
        Check.That(Create([new ApprovalStep(new IdentityRef("Bob"), "approved")]).PendingApprovers).IsEqualTo("-");
    }

    private static Approval Create(List<ApprovalStep>? steps) =>
        new(Guid.NewGuid(),
            "pending",
            DateTimeOffset.UtcNow,
            null,
            1,
            steps,
            new ApprovalPipeline("Cronus - Deploy", new ApprovalRun(1234, "20261103.1")));
}
