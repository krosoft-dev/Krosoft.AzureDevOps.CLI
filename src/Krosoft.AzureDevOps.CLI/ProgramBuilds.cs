using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Managers;

namespace Krosoft.AzureDevOps.CLI;

internal static class ProgramBuilds
{
    public static Task<int> List(Options.BuildListOptions opts) => GetBuildManager().List(opts.Profile);

    private static IBuildManager GetBuildManager() => new BuildManager();
}
