using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Helpers;

internal static class ProjectResolver
{
    // Projets explicitement listés dans le profil, sinon tous les projets de l'organisation.
    // Dans les deux cas, les projets présents dans 'excludedProjects' sont retirés.
    internal static async Task<IReadOnlyList<string>> ResolveAsync(IAzureDevOpsClient client, AzureDevOpsProfile profile)
    {
        var configured = Clean(profile.Projects);

        var projects = configured.Count > 0
            ? configured
            : (await client.GetProjectsAsync()).Select(p => p.Name).ToList();

        var excluded = new HashSet<string>(Clean(profile.ExcludedProjects), StringComparer.OrdinalIgnoreCase);
        if (excluded.Count == 0)
        {
            return projects;
        }

        return projects.Where(p => !excluded.Contains(p)).ToList();
    }

    private static List<string> Clean(IEnumerable<string>? values) =>
        (values ?? [])
        .Where(p => !string.IsNullOrWhiteSpace(p))
        .Select(p => p.Trim())
        .ToList();
}
