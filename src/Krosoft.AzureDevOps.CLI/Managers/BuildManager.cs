using Krosoft.AzureDevOps.CLI.Clients;
using Krosoft.AzureDevOps.CLI.Helpers;
using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Managers;

internal class BuildManager : IBuildManager
{
    public async Task<int> List(string profilePath)
    {
        var (profile, error) = await ProfileLoader.LoadAsync(profilePath);
        if (profile is null)
        {
            return ConsoleHelper.HandleError(error!);
        }

        ConsoleHelper.DisplayHeader($"BUILDS EN COURS ET EN ATTENTE - {profile.Name}");

        try
        {
            using IAzureDevOpsClient client = new AzureDevOpsClient(profile.AzureDevOps);

            var projects = await ProjectResolver.ResolveAsync(client, profile.AzureDevOps);
            Console.WriteLine($"Projets : {projects.Count} ({string.Join(", ", projects)})");
            Console.WriteLine();

            var builds = new List<Build>();
            foreach (var project in projects)
            {
                builds.AddRange(await client.GetActiveBuildsAsync(project));
            }

            if (builds.Count == 0)
            {
                Console.WriteLine("Aucun build en cours ou en attente.");
                return 0;
            }

            Display(client, builds.OrderBy(b => b.Status, StringComparer.OrdinalIgnoreCase).ThenBy(b => b.QueueTime).ToList());
            return 0;
        }
        catch (Exception ex)
        {
            return ConsoleHelper.HandleError($"Impossible de lister les builds : {ex.Message}");
        }
    }

    private static void Display(IAzureDevOpsClient client, IReadOnlyList<Build> builds)
    {
        Console.WriteLine($"{"#",-4} {"ID",-8} {"Projet",-20} {"Pipeline",-30} {"Statut",-12} {"Branche",-30} {"PR",-7} {"En file depuis",-17} Demandé par");
        Console.WriteLine(new string('─', ConsoleHelper.Width));

        var index = 1;
        foreach (var build in builds)
        {
            Console.WriteLine($"{index,-4} {build.Id,-8} {ConsoleHelper.Truncate(build.Project.Name, 20),-20} " +
                              $"{ConsoleHelper.Truncate(build.Definition.Name, 30),-30} {build.Status,-12} " +
                              $"{ConsoleHelper.Truncate(build.ShortBranch, 30),-30} {build.PullRequestNumber ?? "-",-7} " +
                              $"{build.QueueTime.LocalDateTime,-17:yyyy-MM-dd HH:mm} {build.RequestedFor?.DisplayName ?? "-"}");
            Console.WriteLine($"     {client.GetBuildUrl(build)}");
            index++;
        }

        Console.WriteLine(new string('─', ConsoleHelper.Width));

        var inProgress = builds.Count(b => string.Equals(b.Status, "inProgress", StringComparison.OrdinalIgnoreCase));
        Console.WriteLine($"Total : {builds.Count} build(s), {inProgress} en cours, {builds.Count - inProgress} en attente");
    }
}
