namespace Krosoft.AzureDevOps.CLI.Interfaces;

internal interface IBuildManager
{
    Task<int> List(string profilePath);
}
