using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Managers;

namespace Krosoft.AzureDevOps.CLI;

internal static class ProgramPolicies
{
    public static Task<int> List(Options.PolicyListOptions opts) => GetPolicyManager().List(opts.Profile, opts.Type, opts.Repository);

    private static IPolicyManager GetPolicyManager() => new PolicyManager();
}
