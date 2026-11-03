using Krosoft.AzureDevOps.CLI.Clients;
using Krosoft.AzureDevOps.CLI.Helpers;
using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Managers;

internal class ApprovalManager : IApprovalManager
{
    public async Task<int> List(string profilePath)
    {
        var (profile, error) = await ProfileLoader.LoadAsync(profilePath);
        if (profile is null)
        {
            return ConsoleHelper.HandleError(error!);
        }

        ConsoleHelper.DisplayHeader($"VALIDATIONS EN ATTENTE - {profile.Name}");

        try
        {
            using IAzureDevOpsClient client = new AzureDevOpsClient(profile.AzureDevOps);

            var projects = await ProjectResolver.ResolveAsync(client, profile.AzureDevOps);
            Console.WriteLine($"Projets : {projects.Count} ({string.Join(", ", projects)})");
            Console.WriteLine();

            var rows = new List<(string Project, Approval Approval, Build? Build)>();
            foreach (var project in projects)
            {
                var approvals = await client.GetPendingApprovalsAsync(project);
                if (approvals.Count == 0)
                {
                    continue;
                }

                // Un run en attente de validation est "inProgress" : les builds actifs donnent la branche et le demandeur.
                var builds = (await client.GetActiveBuildsAsync(project)).ToDictionary(b => b.Id);
                rows.AddRange(approvals.Select(a => (project, a, builds.GetValueOrDefault(a.Pipeline.Owner.Id))));
            }

            if (rows.Count == 0)
            {
                Console.WriteLine("Aucune validation en attente.");
                return 0;
            }

            Display(client, rows.OrderBy(r => r.Approval.CreatedOn).ToList());
            return 0;
        }
        catch (Exception ex)
        {
            return ConsoleHelper.HandleError($"Impossible de lister les validations : {ex.Message}");
        }
    }

    private static void Display(IAzureDevOpsClient client, IReadOnlyList<(string Project, Approval Approval, Build? Build)> rows)
    {
        Console.WriteLine($"{"#",-4} {"Build",-8} {"Projet",-20} {"Pipeline",-30} {"Branche",-30} {"En attente depuis",-17} Approbateurs");
        Console.WriteLine(new string('─', ConsoleHelper.Width));

        var index = 1;
        foreach (var (project, approval, build) in rows)
        {
            Console.WriteLine($"{index,-4} {approval.Pipeline.Owner.Id,-8} {ConsoleHelper.Truncate(project, 20),-20} " +
                              $"{ConsoleHelper.Truncate(approval.Pipeline.Name, 30),-30} {ConsoleHelper.Truncate(build?.ShortBranch ?? "-", 30),-30} " +
                              $"{approval.CreatedOn.LocalDateTime,-17:yyyy-MM-dd HH:mm} {approval.PendingApprovers}");

            if (!string.IsNullOrWhiteSpace(approval.Instructions))
            {
                Console.WriteLine($"     Instructions : {approval.Instructions.Trim()}");
            }

            Console.WriteLine($"     {client.GetBuildUrl(project, approval.Pipeline.Owner.Id)}");
            index++;
        }

        Console.WriteLine(new string('─', ConsoleHelper.Width));
        Console.WriteLine($"Total : {rows.Count} validation(s) en attente sur {rows.Select(r => r.Approval.Pipeline.Owner.Id).Distinct().Count()} build(s)");
    }
}
