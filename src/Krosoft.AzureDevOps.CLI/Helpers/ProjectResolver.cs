using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Helpers;

internal static class ProjectResolver
{
    // Projets explicitement listés dans le profil, sinon tous les projets de l'organisation.
    internal static async Task<IReadOnlyList<string>> ResolveAsync(IAzureDevOpsClient client, AzureDevOpsProfile profile)
    {
        var configured = (profile.Projects ?? [])
                         .Where(p => !string.IsNullOrWhiteSpace(p))
                         .Select(p => p.Trim())
                         .ToList();

        if (configured.Count > 0)
        {
            return configured;
        }

        var projects = await client.GetProjectsAsync();
        return projects.Select(p => p.Name).ToList();
    }
}
