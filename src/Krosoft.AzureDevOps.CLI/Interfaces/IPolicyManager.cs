namespace Krosoft.AzureDevOps.CLI.Interfaces;

internal interface IPolicyManager
{
    Task<int> List(string profilePath, string? type, string? repository);
}
