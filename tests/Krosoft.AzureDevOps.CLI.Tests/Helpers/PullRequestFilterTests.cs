using Krosoft.AzureDevOps.CLI.Helpers;
using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Tests.Helpers;

[TestClass]
public class PullRequestFilterTests
{
    private static readonly List<PullRequest> PullRequests =
    [
        Create(1, "Renovate - Update all Krosoft.Extensions packages", "Repo.B", new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero)),
        Create(2, "Renovate - Update all Krosoft.Extensions packages", "Repo.A", new DateTimeOffset(2026, 9, 2, 8, 0, 0, TimeSpan.Zero)),
        Create(3, "renovate - update all krosoft.extensions packages (major)", "Repo.A", new DateTimeOffset(2026, 8, 30, 8, 0, 0, TimeSpan.Zero)),
        Create(4, "Feature/login", "Repo.C", new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero))
    ];

    [TestMethod]
    public void Apply_SansFiltre_RetourneToutTrieParDepotPuisDate()
    {
        var result = PullRequestFilter.Apply(PullRequests, PullRequestFilters.Default);

        Check.That(result.Select(pr => pr.Id)).ContainsExactly(2, 3, 1, 4);
    }

    [TestMethod]
    public void Apply_TitreContient_InsensibleALaCasse()
    {
        var filters = new PullRequestFilters(Titles: ["krosoft.extensions"]);

        var result = PullRequestFilter.Apply(PullRequests, filters);

        Check.That(result.Select(pr => pr.Id)).ContainsExactly(2, 3, 1);
    }

    [TestMethod]
    public void Apply_PlusieursTitres_CombineEnOu()
    {
        var filters = new PullRequestFilters(Titles: ["(major)", "Feature/"]);

        var result = PullRequestFilter.Apply(PullRequests, filters);

        Check.That(result.Select(pr => pr.Id)).ContainsExactly(3, 4);
    }

    [TestMethod]
    public void Apply_TitreExact_ExclutLesVariantes()
    {
        var filters = new PullRequestFilters(Titles: ["Renovate - Update all Krosoft.Extensions packages"], ExactTitle: true);

        var result = PullRequestFilter.Apply(PullRequests, filters);

        Check.That(result.Select(pr => pr.Id)).ContainsExactly(2, 1);
    }

    [TestMethod]
    public void Apply_Depots_LimiteAuxDepotsDemandes()
    {
        var filters = new PullRequestFilters(Titles: ["Renovate"], Repositories: ["repo.a", "Repo.C"]);

        var result = PullRequestFilter.Apply(PullRequests, filters);

        Check.That(result.Select(pr => pr.Id)).ContainsExactly(2, 3);
    }

    [TestMethod]
    public void Apply_ValeursVides_SontIgnorees()
    {
        var filters = new PullRequestFilters(Titles: ["", "  "], Repositories: [" "]);

        var result = PullRequestFilter.Apply(PullRequests, filters);

        Check.That(result).HasSize(4);
    }

    [TestMethod]
    public void ByIds_RetourneUniquementLesIdsDemandes_IgnoreLesInconnus()
    {
        var result = PullRequestFilter.ByIds(PullRequests, [4, 1, 999]);

        Check.That(result.Select(pr => pr.Id)).ContainsExactly(1, 4);
    }

    [TestMethod]
    public void ByIds_ListeVide_NeRetourneRien()
    {
        var result = PullRequestFilter.ByIds(PullRequests, []);

        Check.That(result).IsEmpty();
    }

    private static PullRequest Create(int id, string title, string repository, DateTimeOffset creationDate) =>
        new(id,
            title,
            "active",
            false,
            creationDate,
            "refs/heads/renovate/all",
            "refs/heads/main",
            new IdentityRef("Renovate Bot"),
            new GitRepository(Guid.NewGuid().ToString(), repository, new TeamProject(Guid.NewGuid().ToString(), "Proj")));
}
