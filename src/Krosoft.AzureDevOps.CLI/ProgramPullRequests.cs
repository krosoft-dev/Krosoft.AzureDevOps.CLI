using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Managers;

namespace Krosoft.AzureDevOps.CLI;

internal static class ProgramPullRequests
{
    public static Task<int> List(Options.PullRequestsOptions opts) => GetPullRequestManager().List(opts.Profile);

    public static Task<int> Approve(Options.ApproveOptions opts) =>
        GetPullRequestManager().Approve(opts.Profile, opts.DryRun, opts.Ids.ToList());

    public static Task<int> Requeue(Options.RequeueOptions opts) =>
        GetPullRequestManager().Requeue(opts.Profile, opts.DryRun, opts.Ids.ToList());

    private static IPullRequestManager GetPullRequestManager() => new PullRequestManager();
}
