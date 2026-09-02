using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Helpers;

internal static class PullRequestFilter
{
    internal static IReadOnlyList<PullRequest> Apply(IEnumerable<PullRequest> pullRequests, PullRequestFilters filters)
    {
        var query = pullRequests;

        var titles = Clean(filters.Titles);
        if (titles.Count > 0)
        {
            query = filters.ExactTitle
                ? query.Where(pr => titles.Any(t => string.Equals(pr.Title.Trim(), t, StringComparison.OrdinalIgnoreCase)))
                : query.Where(pr => titles.Any(t => pr.Title.Contains(t, StringComparison.OrdinalIgnoreCase)));
        }

        var repositories = Clean(filters.Repositories);
        if (repositories.Count > 0)
        {
            query = query.Where(pr => repositories.Contains(pr.Repository.Name, StringComparer.OrdinalIgnoreCase));
        }

        return query.OrderBy(pr => pr.Repository.Project.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(pr => pr.Repository.Name, StringComparer.OrdinalIgnoreCase)
                    .ThenByDescending(pr => pr.CreationDate)
                    .ToList();
    }

    // Sélection explicite par ID (unique dans l'organisation) : ignore les filtres titre/dépôt.
    internal static IReadOnlyList<PullRequest> ByIds(IEnumerable<PullRequest> pullRequests, IReadOnlyCollection<int> ids)
    {
        var wanted = ids.ToHashSet();
        return pullRequests.Where(pr => wanted.Contains(pr.Id))
                           .OrderBy(pr => pr.Repository.Project.Name, StringComparer.OrdinalIgnoreCase)
                           .ThenBy(pr => pr.Repository.Name, StringComparer.OrdinalIgnoreCase)
                           .ThenByDescending(pr => pr.CreationDate)
                           .ToList();
    }

    private static List<string> Clean(IEnumerable<string>? values) =>
        values is null
            ? []
            : values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToList();
}
