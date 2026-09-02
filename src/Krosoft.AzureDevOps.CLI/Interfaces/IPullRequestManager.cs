namespace Krosoft.AzureDevOps.CLI.Interfaces;

internal interface IPullRequestManager
{
    Task<int> List(string profilePath);

    Task<int> Approve(string profilePath, bool dryRun, IReadOnlyCollection<int> ids);

    Task<int> Requeue(string profilePath, bool dryRun, IReadOnlyCollection<int> ids);
}
