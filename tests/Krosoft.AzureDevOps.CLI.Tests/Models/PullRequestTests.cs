using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Tests.Models;

[TestClass]
public class PullRequestTests
{
    private const string Me = "11111111-1111-1111-1111-111111111111";
    private const string Other = "22222222-2222-2222-2222-222222222222";

    [TestMethod]
    public void IsApprovedBy_SansReviewers_Faux()
    {
        var pr = Create(null);

        Check.That(pr.IsApprovedBy(Me)).IsFalse();
        Check.That(pr.VoteOf(Me)).IsEqualTo(Vote.NoVote);
    }

    [TestMethod]
    public void IsApprovedBy_ApprouveParUnAutre_Faux()
    {
        var pr = Create([new Reviewer(Other, "Autre", Vote.Approved)]);

        Check.That(pr.IsApprovedBy(Me)).IsFalse();
    }

    [TestMethod]
    public void IsApprovedBy_ApprouveParMoi_Vrai()
    {
        var pr = Create([new Reviewer(Other, "Autre", Vote.Rejected), new Reviewer(Me.ToUpperInvariant(), "Moi", Vote.Approved)]);

        Check.That(pr.IsApprovedBy(Me)).IsTrue();
        Check.That(pr.VoteOf(Me)).IsEqualTo(Vote.Approved);
    }

    [TestMethod]
    public void IsApprovedBy_ApprouveAvecSuggestions_Vrai()
    {
        var pr = Create([new Reviewer(Me, "Moi", Vote.ApprovedWithSuggestions)]);

        Check.That(pr.IsApprovedBy(Me)).IsTrue();
    }

    [TestMethod]
    public void IsApprovedBy_EnAttenteDeLAuteur_Faux()
    {
        var pr = Create([new Reviewer(Me, "Moi", Vote.WaitingForAuthor)]);

        Check.That(pr.IsApprovedBy(Me)).IsFalse();
    }

    [TestMethod]
    public void IsAutoCompleteSet_SansAutoCompleteSetBy_Faux()
    {
        var pr = Create(null);

        Check.That(pr.IsAutoCompleteSet).IsFalse();
    }

    [TestMethod]
    public void IsAutoCompleteSet_AvecAutoCompleteSetBy_Vrai()
    {
        var pr = Create(null, new IdentityRef("Moi"));

        Check.That(pr.IsAutoCompleteSet).IsTrue();
    }

    private static PullRequest Create(List<Reviewer>? reviewers, IdentityRef? autoCompleteSetBy = null) =>
        new(1,
            "Renovate - Update all Krosoft.Extensions packages",
            "active",
            false,
            DateTimeOffset.UtcNow,
            "refs/heads/renovate/all",
            "refs/heads/main",
            new IdentityRef("Renovate Bot"),
            new GitRepository(Guid.NewGuid().ToString(), "Repo", new TeamProject(Guid.NewGuid().ToString(), "Proj")),
            reviewers,
            autoCompleteSetBy);
}
