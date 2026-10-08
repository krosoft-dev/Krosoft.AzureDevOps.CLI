using Krosoft.AzureDevOps.CLI.Helpers;
using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Tests.Helpers;

[TestClass]
public class ProjectResolverTests
{
    [TestMethod]
    public async Task ResolveAsync_ProjetsListes_RetourneLaListe()
    {
        var client = CreateClient("Alpha", "Beta", "Gamma");
        var profile = CreateProfile(projects: ["Alpha", "Beta"]);

        var result = await ProjectResolver.ResolveAsync(client.Object, profile);

        Check.That(result).ContainsExactly("Alpha", "Beta");
        client.Verify(c => c.GetProjectsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task ResolveAsync_ProjetsVides_RetourneTousLesProjetsDeLOrga()
    {
        var client = CreateClient("Alpha", "Beta", "Gamma");
        var profile = CreateProfile();

        var result = await ProjectResolver.ResolveAsync(client.Object, profile);

        Check.That(result).ContainsExactly("Alpha", "Beta", "Gamma");
    }

    [TestMethod]
    public async Task ResolveAsync_TousLesProjets_ExcluCeuxDeLaListeDExclusion()
    {
        var client = CreateClient("Alpha", "Beta", "Gamma");
        var profile = CreateProfile(excludedProjects: ["Beta"]);

        var result = await ProjectResolver.ResolveAsync(client.Object, profile);

        Check.That(result).ContainsExactly("Alpha", "Gamma");
    }

    [TestMethod]
    public async Task ResolveAsync_ProjetsListes_ExcluCeuxDeLaListeDExclusion()
    {
        var client = CreateClient("Alpha", "Beta", "Gamma");
        var profile = CreateProfile(projects: ["Alpha", "Beta"], excludedProjects: ["Beta"]);

        var result = await ProjectResolver.ResolveAsync(client.Object, profile);

        Check.That(result).ContainsExactly("Alpha");
    }

    [TestMethod]
    public async Task ResolveAsync_Exclusion_InsensibleALaCasseEtAuxEspaces()
    {
        var client = CreateClient("Alpha", "Beta", "Gamma");
        var profile = CreateProfile(excludedProjects: ["  beta  "]);

        var result = await ProjectResolver.ResolveAsync(client.Object, profile);

        Check.That(result).ContainsExactly("Alpha", "Gamma");
    }

    private static Mock<IAzureDevOpsClient> CreateClient(params string[] projectNames)
    {
        var mock = new Mock<IAzureDevOpsClient>();
        var projects = projectNames.Select(n => new TeamProject(Guid.NewGuid().ToString(), n)).ToList();
        mock.Setup(c => c.GetProjectsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(projects);
        return mock;
    }

    private static AzureDevOpsProfile CreateProfile(List<string>? projects = null, List<string>? excludedProjects = null) =>
        new("https://dev.azure.com/org", "secret", projects, excludedProjects);
}
