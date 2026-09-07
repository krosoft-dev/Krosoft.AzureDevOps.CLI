using Krosoft.AzureDevOps.CLI.Clients;
using Krosoft.AzureDevOps.CLI.Helpers;
using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Managers;

internal class PullRequestManager : IPullRequestManager
{
    public async Task<int> List(string profilePath)
    {
        var (profile, error) = await ProfileLoader.LoadAsync(profilePath);
        if (profile is null)
        {
            return ConsoleHelper.HandleError(error!);
        }

        ConsoleHelper.DisplayHeader($"PULL REQUESTS - {profile.Name}");

        try
        {
            using IAzureDevOpsClient client = new AzureDevOpsClient(profile.AzureDevOps);

            var (projects, all, pullRequests) = await FetchAsync(client, profile);
            if (pullRequests.Count == 0)
            {
                Console.WriteLine($"Aucune pull request trouvée ({all.Count} PR(s) analysée(s) dans {projects.Count} projet(s)).");
                return 0;
            }

            Display(client, pullRequests, all.Count, projects.Count);
            return 0;
        }
        catch (Exception ex)
        {
            return ConsoleHelper.HandleError($"Impossible de lister les pull requests : {ex.Message}");
        }
    }

    public async Task<int> Approve(string profilePath, bool dryRun, bool complete, IReadOnlyCollection<int> ids)
    {
        var (profile, error) = await ProfileLoader.LoadAsync(profilePath);
        if (profile is null)
        {
            return ConsoleHelper.HandleError(error!);
        }

        var title = complete ? "APPROBATION + AUTO-COMPLETE" : "APPROBATION";
        ConsoleHelper.DisplayHeader(dryRun ? $"{title} (SIMULATION) - {profile.Name}" : $"{title} - {profile.Name}");

        try
        {
            using IAzureDevOpsClient client = new AzureDevOpsClient(profile.AzureDevOps);

            var userId = await client.GetCurrentUserIdAsync();
            var (projects, all, pullRequests) = await FetchAsync(client, profile, ids);

            var missingIds = ids.Except(pullRequests.Select(pr => pr.Id)).ToList();
            if (missingIds.Count > 0)
            {
                ConsoleHelper.WriteColoredLine(ConsoleColor.Yellow,
                                               $"ID(s) introuvable(s) parmi les PR au statut '{(profile.PullRequests ?? PullRequestFilters.Default).Status.ToString().ToLowerInvariant()}' : {string.Join(", ", missingIds)}");
                Console.WriteLine();
            }

            if (pullRequests.Count == 0)
            {
                Console.WriteLine($"Aucune pull request trouvée ({all.Count} PR(s) analysée(s) dans {projects.Count} projet(s)).");
                return missingIds.Count > 0 ? -1 : 0;
            }

            var toApprove = pullRequests.Where(pr => !pr.IsApprovedBy(userId)).ToList();
            List<PullRequest> toComplete = complete ? pullRequests.Where(pr => !pr.IsAutoCompleteSet).ToList() : [];
            var actionable = pullRequests.Where(pr => !pr.IsApprovedBy(userId) || (complete && !pr.IsAutoCompleteSet)).ToList();
            var alreadyApproved = pullRequests.Count - toApprove.Count;

            Console.WriteLine($"{"#",-4} {"ID",-7} {"Projet",-20} {"Dépôt",-24} {"Titre",-44} {"Mon vote",-28} Action");
            Console.WriteLine(new string('─', ConsoleHelper.Width));

            var index = 1;
            foreach (var pr in pullRequests)
            {
                Console.WriteLine($"{index,-4} {pr.Id,-7} {ConsoleHelper.Truncate(pr.Repository.Project.Name, 20),-20} " +
                                  $"{ConsoleHelper.Truncate(pr.Repository.Name, 24),-24} {ConsoleHelper.Truncate(pr.Title, 44),-44} " +
                                  $"{Vote.ToLabel(pr.VoteOf(userId)),-28} {DescribeAction(pr, userId, complete, dryRun)}");
                Console.WriteLine($"     {client.GetPullRequestUrl(pr)}");
                index++;
            }

            Console.WriteLine(new string('─', ConsoleHelper.Width));
            var summary = $"{pullRequests.Count} pull request(s) correspondante(s) sur {all.Count} analysée(s) dans {projects.Count} projet(s) : {toApprove.Count} à approuver";
            if (complete)
            {
                summary += $", {toComplete.Count} à compléter";
            }

            summary += $", {alreadyApproved} déjà approuvée(s).";
            Console.WriteLine(summary);
            Console.WriteLine();

            if (dryRun)
            {
                ConsoleHelper.WriteColoredLine(ConsoleColor.Yellow, "Mode simulation : aucune modification effectuée. Relancer sans --dry-run pour appliquer.");
                return 0;
            }

            if (actionable.Count == 0)
            {
                ConsoleHelper.WriteColoredLine(ConsoleColor.Green, complete ? "Rien à approuver ni à compléter." : "Rien à approuver.");
                return 0;
            }

            var approved = 0;
            var completed = 0;
            var failures = 0;
            foreach (var pr in actionable)
            {
                var label = $"!{pr.Id} {pr.Repository.Project.Name}/{pr.Repository.Name}";

                if (!pr.IsApprovedBy(userId))
                {
                    try
                    {
                        await client.ApproveAsync(pr, userId);
                        approved++;
                        ConsoleHelper.WriteColoredLine(ConsoleColor.Green, $"  [OK] {label} approuvée");
                    }
                    catch (Exception ex)
                    {
                        failures++;
                        ConsoleHelper.WriteColoredLine(ConsoleColor.Red, $"  [KO] {label} : {ex.Message}");
                        continue; // Approbation échouée : inutile de tenter l'auto-complétion.
                    }
                }

                if (complete && !pr.IsAutoCompleteSet)
                {
                    try
                    {
                        await client.SetAutoCompleteAsync(pr, userId);
                        completed++;
                        ConsoleHelper.WriteColoredLine(ConsoleColor.Green, $"  [OK] {label} auto-complétion activée");
                    }
                    catch (Exception ex)
                    {
                        failures++;
                        ConsoleHelper.WriteColoredLine(ConsoleColor.Red, $"  [KO] {label} auto-complétion : {ex.Message}");
                    }
                }
            }

            Console.WriteLine();
            if (failures > 0)
            {
                return ConsoleHelper.HandleError(complete
                                                     ? $"{approved} approuvée(s), {completed} en auto-complete, {failures} en échec."
                                                     : $"{approved} approuvée(s), {failures} en échec.");
            }

            ConsoleHelper.WriteColoredLine(ConsoleColor.Green, complete
                                               ? $"{approved} pull request(s) approuvée(s), {completed} passée(s) en auto-complete."
                                               : $"{approved} pull request(s) approuvée(s).");
            return 0;
        }
        catch (Exception ex)
        {
            return ConsoleHelper.HandleError($"Impossible d'approuver les pull requests : {ex.Message}");
        }
    }

    // Décrit l'action prévue pour une PR dans le tableau récapitulatif (approbation et/ou auto-complétion).
    private static string DescribeAction(PullRequest pr, string userId, bool complete, bool dryRun)
    {
        var parts = new List<string>();
        if (!pr.IsApprovedBy(userId))
        {
            parts.Add(dryRun ? "à approuver" : "approbation...");
        }

        if (complete && !pr.IsAutoCompleteSet)
        {
            parts.Add(dryRun ? "à compléter" : "auto-complétion...");
        }

        return parts.Count > 0 ? string.Join(" + ", parts) : "ignorée (déjà approuvée)";
    }

    public async Task<int> Requeue(string profilePath, bool dryRun, IReadOnlyCollection<int> ids)
    {
        var (profile, error) = await ProfileLoader.LoadAsync(profilePath);
        if (profile is null)
        {
            return ConsoleHelper.HandleError(error!);
        }

        ConsoleHelper.DisplayHeader(dryRun
                                        ? $"RELANCE DES BUILDS (SIMULATION) - {profile.Name}"
                                        : $"RELANCE DES BUILDS - {profile.Name}");

        try
        {
            using IAzureDevOpsClient client = new AzureDevOpsClient(profile.AzureDevOps);

            var (projects, all, pullRequests) = await FetchAsync(client, profile, ids);

            var missingIds = ids.Except(pullRequests.Select(pr => pr.Id)).ToList();
            if (missingIds.Count > 0)
            {
                ConsoleHelper.WriteColoredLine(ConsoleColor.Yellow, $"ID(s) introuvable(s) : {string.Join(", ", missingIds)}");
                Console.WriteLine();
            }

            if (pullRequests.Count == 0)
            {
                Console.WriteLine($"Aucune pull request trouvée ({all.Count} PR(s) analysée(s) dans {projects.Count} projet(s)).");
                return missingIds.Count > 0 ? -1 : 0;
            }

            // Inspection des policies de chaque PR : on ne relance que les builds en échec.
            var plan = new List<(PullRequest pr, PolicyEvaluation evaluation)>();
            var index = 1;
            foreach (var pr in pullRequests)
            {
                Console.WriteLine($"{index,-4} !{pr.Id,-7} {pr.Repository.Project.Name}/{pr.Repository.Name}  {ConsoleHelper.Truncate(pr.Title, 60)}");
                Console.WriteLine($"     {client.GetPullRequestUrl(pr)}");

                var evaluations = await client.GetPolicyEvaluationsAsync(pr);
                var failed = evaluations.Where(e => e.Configuration.IsEnabled && e.IsFailed).ToList();

                if (failed.Count == 0)
                {
                    ConsoleHelper.WriteColoredLine(ConsoleColor.DarkGray, "     aucune policy en échec");
                }

                foreach (var evaluation in failed)
                {
                    var blocking = evaluation.Configuration.IsBlocking ? "requise" : "optionnelle";
                    if (evaluation.CanRequeue)
                    {
                        plan.Add((pr, evaluation));
                        ConsoleHelper.WriteColoredLine(ConsoleColor.Yellow, $"     [{evaluation.Status}] {evaluation.Name} ({blocking}) -> {(dryRun ? "à relancer" : "relance")}");
                    }
                    else
                    {
                        ConsoleHelper.WriteColoredLine(ConsoleColor.DarkGray, $"     [{evaluation.Status}] {evaluation.Name} ({blocking}) -> non relançable, à traiter manuellement");
                    }
                }

                index++;
            }

            Console.WriteLine(new string('─', ConsoleHelper.Width));
            Console.WriteLine($"{pullRequests.Count} pull request(s) inspectée(s) sur {all.Count} analysée(s) dans {projects.Count} projet(s) : {plan.Count} build(s) à relancer.");
            Console.WriteLine();

            if (dryRun)
            {
                ConsoleHelper.WriteColoredLine(ConsoleColor.Yellow, "Mode simulation : aucun build relancé. Relancer sans --dry-run pour exécuter.");
                return 0;
            }

            if (plan.Count == 0)
            {
                ConsoleHelper.WriteColoredLine(ConsoleColor.Green, "Rien à relancer.");
                return 0;
            }

            var failures = 0;
            foreach (var (pr, evaluation) in plan)
            {
                try
                {
                    await client.RequeuePolicyEvaluationAsync(pr, evaluation.EvaluationId);
                    ConsoleHelper.WriteColoredLine(ConsoleColor.Green, $"  [OK] !{pr.Id} {pr.Repository.Project.Name}/{pr.Repository.Name} : {evaluation.Name} relancé");
                }
                catch (Exception ex)
                {
                    failures++;
                    ConsoleHelper.WriteColoredLine(ConsoleColor.Red, $"  [KO] !{pr.Id} {pr.Repository.Project.Name}/{pr.Repository.Name} : {evaluation.Name} : {ex.Message}");
                }
            }

            Console.WriteLine();
            if (failures > 0)
            {
                return ConsoleHelper.HandleError($"{plan.Count - failures} relancé(s), {failures} en échec.");
            }

            ConsoleHelper.WriteColoredLine(ConsoleColor.Green, $"{plan.Count} build(s) relancé(s).");
            return 0;
        }
        catch (Exception ex)
        {
            return ConsoleHelper.HandleError($"Impossible de relancer les builds : {ex.Message}");
        }
    }

    // Résout les projets, récupère toutes les PR du statut demandé, puis applique les filtres du profil
    // (ou la sélection explicite par ID si elle est fournie).
    private static async Task<(IReadOnlyList<string> projects, IReadOnlyList<PullRequest> all, IReadOnlyList<PullRequest> filtered)> FetchAsync(
        IAzureDevOpsClient client,
        Profile profile,
        IReadOnlyCollection<int>? ids = null)
    {
        var filters = profile.PullRequests ?? PullRequestFilters.Default;
        var byIds = ids is { Count: > 0 };

        var projects = await ProjectResolver.ResolveAsync(client, profile.AzureDevOps);
        DisplayFilters(projects, filters, byIds ? ids : null);

        var all = new List<PullRequest>();
        foreach (var project in projects)
        {
            all.AddRange(await client.GetPullRequestsAsync(project, filters.Status));
        }

        var filtered = byIds
            ? PullRequestFilter.ByIds(all, ids!)
            : PullRequestFilter.Apply(all, filters);

        return (projects, all, filtered);
    }

    private static void DisplayFilters(IReadOnlyList<string> projects, PullRequestFilters filters, IReadOnlyCollection<int>? ids)
    {
        Console.WriteLine($"Projets : {projects.Count} ({string.Join(", ", projects)})");
        Console.WriteLine($"Statut  : {filters.Status.ToString().ToLowerInvariant()}");

        if (ids is { Count: > 0 })
        {
            Console.WriteLine($"IDs     : {string.Join(", ", ids)} (les filtres titre/dépôt du profil sont ignorés)");
            Console.WriteLine();
            return;
        }

        if (filters.Titles is { Count: > 0 })
        {
            Console.WriteLine($"Titre   : {(filters.ExactTitle ? "égal à" : "contient")} {string.Join(" | ", filters.Titles.Select(t => $"\"{t}\""))}");
        }

        if (filters.Repositories is { Count: > 0 })
        {
            Console.WriteLine($"Dépôts  : {string.Join(", ", filters.Repositories)}");
        }

        Console.WriteLine();
    }

    private static void Display(IAzureDevOpsClient client, IReadOnlyList<PullRequest> pullRequests, int totalAnalyzed, int projectCount)
    {
        Console.WriteLine($"{"#",-4} {"ID",-7} {"Projet",-20} {"Dépôt",-24} {"Titre",-44} {"Auteur",-20} {"Créée le",-17} Statut");
        Console.WriteLine(new string('─', ConsoleHelper.Width));

        var index = 1;
        foreach (var pr in pullRequests)
        {
            var draft = pr.IsDraft ? " (draft)" : string.Empty;
            Console.WriteLine($"{index,-4} {pr.Id,-7} {ConsoleHelper.Truncate(pr.Repository.Project.Name, 20),-20} " +
                              $"{ConsoleHelper.Truncate(pr.Repository.Name, 24),-24} {ConsoleHelper.Truncate(pr.Title, 44),-44} " +
                              $"{ConsoleHelper.Truncate(pr.CreatedBy.DisplayName, 20),-20} " +
                              $"{pr.CreationDate.LocalDateTime,-17:yyyy-MM-dd HH:mm} {pr.Status}{draft}");
            Console.WriteLine($"     {client.GetPullRequestUrl(pr)}");
            index++;
        }

        Console.WriteLine(new string('─', ConsoleHelper.Width));
        Console.WriteLine($"Total : {pullRequests.Count} pull request(s) sur {totalAnalyzed} analysée(s) dans {projectCount} projet(s)");
    }
}
