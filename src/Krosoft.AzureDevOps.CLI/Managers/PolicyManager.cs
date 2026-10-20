using Krosoft.AzureDevOps.CLI.Clients;
using Krosoft.AzureDevOps.CLI.Helpers;
using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Managers;

internal class PolicyManager : IPolicyManager
{
    private const string AllRepositories = "(tous les dépôts)";

    public async Task<int> List(string profilePath, string? type, string? repository)
    {
        var (profile, error) = await ProfileLoader.LoadAsync(profilePath);
        if (profile is null)
        {
            return ConsoleHelper.HandleError(error!);
        }

        ConsoleHelper.DisplayHeader($"BRANCH POLICIES - {profile.Name}");

        try
        {
            using IAzureDevOpsClient client = new AzureDevOpsClient(profile.AzureDevOps);

            var projects = await ProjectResolver.ResolveAsync(client, profile.AzureDevOps);
            Console.WriteLine($"Projets : {projects.Count} ({string.Join(", ", projects)})");
            if (!string.IsNullOrWhiteSpace(type))
            {
                Console.WriteLine($"Type    : contient \"{type}\"");
            }

            if (!string.IsNullOrWhiteSpace(repository))
            {
                Console.WriteLine($"Dépôt   : {repository} (+ policies portant sur tout le projet)");
            }

            Console.WriteLine();

            var rows = new List<(string Project, string Repository, string Branch, PolicyConfiguration Policy)>();
            foreach (var project in projects)
            {
                var repositories = (await client.GetRepositoriesAsync(project)).ToDictionary(r => r.Id, r => r.Name, StringComparer.OrdinalIgnoreCase);
                var policies = await client.GetPolicyConfigurationsAsync(project);

                foreach (var policy in policies.Where(p => MatchesType(p, type)))
                {
                    var scopes = policy.Settings?.Scope is { Count: > 0 } s ? s : [new PolicyScope(null, null, null)];
                    foreach (var scope in scopes)
                    {
                        var repositoryName = scope.IsProjectWide
                            ? AllRepositories
                            : repositories.GetValueOrDefault(scope.RepositoryId!, scope.RepositoryId!);

                        if (!MatchesRepository(scope, repositoryName, repository))
                        {
                            continue;
                        }

                        rows.Add((project, repositoryName, scope.BranchLabel, policy));
                    }
                }
            }

            if (rows.Count == 0)
            {
                Console.WriteLine("Aucune policy trouvée.");
                return 0;
            }

            Display(rows.OrderBy(r => r.Project, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.Repository, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.Policy.Type.DisplayName, StringComparer.OrdinalIgnoreCase)
                        .ToList());
            return 0;
        }
        catch (Exception ex)
        {
            return ConsoleHelper.HandleError($"Impossible de lister les policies : {ex.Message}");
        }
    }

    private static bool MatchesType(PolicyConfiguration policy, string? type) =>
        string.IsNullOrWhiteSpace(type) ||
        policy.Type.DisplayName.Contains(type.Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool MatchesRepository(PolicyScope scope, string repositoryName, string? repository) =>
        string.IsNullOrWhiteSpace(repository) ||
        scope.IsProjectWide ||
        string.Equals(repositoryName, repository.Trim(), StringComparison.OrdinalIgnoreCase);

    private static void Display(IReadOnlyList<(string Project, string Repository, string Branch, PolicyConfiguration Policy)> rows)
    {
        Console.WriteLine($"{"#",-4} {"ID",-6} {"Projet",-16} {"Dépôt",-24} {"Branche",-22} {"Type",-30} {"Mode",-9} Active");
        Console.WriteLine(new string('─', ConsoleHelper.Width));

        var index = 1;
        foreach (var (project, repository, branch, policy) in rows)
        {
            var name = policy.Settings?.DisplayName is { Length: > 0 } displayName
                ? $"{policy.Type.DisplayName} ({displayName})"
                : policy.Type.DisplayName;

            Console.WriteLine($"{index,-4} {policy.Id,-6} {ConsoleHelper.Truncate(project, 16),-16} {ConsoleHelper.Truncate(repository, 24),-24} " +
                              $"{ConsoleHelper.Truncate(branch, 22),-22} {ConsoleHelper.Truncate(name, 30),-30} " +
                              $"{(policy.IsBlocking ? "Required" : "Optional"),-9} {(policy.IsEnabled ? "Oui" : "Non")}");
            index++;
        }

        Console.WriteLine(new string('─', ConsoleHelper.Width));

        var required = rows.Count(r => r.Policy.IsBlocking);
        Console.WriteLine($"Total : {rows.Count} ligne(s) ({rows.Select(r => r.Policy.Id).Distinct().Count()} policies), {required} Required, {rows.Count - required} Optional");
    }
}
