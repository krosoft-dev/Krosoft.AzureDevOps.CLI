using CommandLine;

namespace Krosoft.AzureDevOps.CLI;

internal static class Program
{
    private static async Task<int> Main(params string[] args)
    {
        PrintBanner();
        using var parser = new Parser(settings =>
        {
            settings.HelpWriter = Console.Error;
            settings.CaseInsensitiveEnumValues = true;
        });
        return await parser.ParseArguments<Options.PullRequestsOptions, Options.ApproveOptions, Options.RequeueOptions, Options.BuildListOptions>(args)
                           .MapResult(
                                      (Options.PullRequestsOptions opts) => ProgramPullRequests.List(opts),
                                      (Options.ApproveOptions opts) => ProgramPullRequests.Approve(opts),
                                      (Options.RequeueOptions opts) => ProgramPullRequests.Requeue(opts),
                                      (Options.BuildListOptions opts) => ProgramBuilds.List(opts),
                                      _ => Task.FromResult(-1));
    }

    private static void PrintBanner()
    {
        const string banner = """

                                _  __                     __ _
                               | |/ /                    / _| |
                               | ' / _ __ ___  ___  ___ | |_| |_
                               |  < | '__/ _ \/ __|/ _ \|  _| __|
                               | . \| | | (_) \__ \ (_) | | | |_
                               |_|\_\_|  \___/|___/\___/|_|  \__|

                               Azure DevOps CLI Tool

                              """;
        Console.WriteLine(banner);
    }
}
