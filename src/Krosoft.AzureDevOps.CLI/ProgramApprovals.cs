using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Managers;

namespace Krosoft.AzureDevOps.CLI;

internal static class ProgramApprovals
{
    public static Task<int> List(Options.ApprovalListOptions opts) => GetApprovalManager().List(opts.Profile);

    private static IApprovalManager GetApprovalManager() => new ApprovalManager();
}
