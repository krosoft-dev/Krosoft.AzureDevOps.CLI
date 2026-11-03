namespace Krosoft.AzureDevOps.CLI.Interfaces;

internal interface IApprovalManager
{
    Task<int> List(string profilePath);
}
