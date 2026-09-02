using Krosoft.AzureDevOps.CLI.Helpers;
using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Tests.Helpers;

[TestClass]
public class ProfileLoaderTests
{
    [TestMethod]
    public async Task LoadAsync_ProfilComplet_LitLesFiltres()
    {
        var path = await WriteTempProfile("""
            {
              "name": "test",
              "azureDevOps": { "organizationUrl": "https://dev.azure.com/org", "pat": "secret", "projects": ["Proj"] },
              "pullRequests": {
                "status": "completed",
                "titles": ["Renovate - Update all Krosoft.Extensions packages"],
                "exactTitle": true,
                "repositories": ["Repo.A"]
              }
            }
            """);

        var (profile, error) = await ProfileLoader.LoadAsync(path);

        Check.That(error).IsNull();
        Check.That(profile).IsNotNull();
        Check.That(profile!.PullRequests).IsNotNull();
        Check.That(profile.PullRequests!.Status).IsEqualTo(PullRequestStatus.Completed);
        Check.That(profile.PullRequests.Titles).ContainsExactly("Renovate - Update all Krosoft.Extensions packages");
        Check.That(profile.PullRequests.ExactTitle).IsTrue();
        Check.That(profile.PullRequests.Repositories).ContainsExactly("Repo.A");
    }

    [TestMethod]
    public async Task LoadAsync_SansSectionPullRequests_ProfilValide()
    {
        var path = await WriteTempProfile("""
            {
              "name": "test",
              "azureDevOps": { "organizationUrl": "https://dev.azure.com/org", "pat": "secret", "projects": ["Proj"] }
            }
            """);

        var (profile, error) = await ProfileLoader.LoadAsync(path);

        Check.That(error).IsNull();
        Check.That(profile!.PullRequests).IsNull();
    }

    [TestMethod]
    public async Task LoadAsync_StatutInconnu_RetourneUneErreur()
    {
        var path = await WriteTempProfile("""
            {
              "name": "test",
              "azureDevOps": { "organizationUrl": "https://dev.azure.com/org", "pat": "secret", "projects": ["Proj"] },
              "pullRequests": { "status": "merged" }
            }
            """);

        var (profile, error) = await ProfileLoader.LoadAsync(path);

        Check.That(profile).IsNull();
        Check.That(error).StartsWith("Erreur de lecture du profil");
    }

    [TestMethod]
    public async Task LoadAsync_PatManquant_RetourneUneErreur()
    {
        var path = await WriteTempProfile("""
            {
              "name": "test",
              "azureDevOps": { "organizationUrl": "https://dev.azure.com/org" }
            }
            """);

        var (profile, error) = await ProfileLoader.LoadAsync(path);

        Check.That(profile).IsNull();
        Check.That(error).Contains("azureDevOps.pat");
    }

    [TestMethod]
    public async Task LoadAsync_ProjetsAbsents_ProfilValide()
    {
        var path = await WriteTempProfile("""
            {
              "name": "test",
              "azureDevOps": { "organizationUrl": "https://dev.azure.com/org", "pat": "secret" }
            }
            """);

        var (profile, error) = await ProfileLoader.LoadAsync(path);

        Check.That(error).IsNull();
        Check.That(profile!.AzureDevOps.Projects).IsNull();
    }

    private static async Task<string> WriteTempProfile(string json)
    {
        var path = Path.Combine(Path.GetTempPath(), $"krosoft-devops-{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, json);
        return path;
    }
}
