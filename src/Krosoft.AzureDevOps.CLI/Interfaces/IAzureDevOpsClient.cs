using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Interfaces;

internal interface IAzureDevOpsClient : IDisposable
{
    Task<IReadOnlyList<TeamProject>> GetProjectsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PullRequest>> GetPullRequestsAsync(string project, PullRequestStatus status, CancellationToken cancellationToken = default);

    Task<string> GetCurrentUserIdAsync(CancellationToken cancellationToken = default);

    Task ApproveAsync(PullRequest pullRequest, string reviewerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PolicyEvaluation>> GetPolicyEvaluationsAsync(PullRequest pullRequest, CancellationToken cancellationToken = default);

    Task RequeuePolicyEvaluationAsync(PullRequest pullRequest, Guid evaluationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Build>> GetActiveBuildsAsync(string project, CancellationToken cancellationToken = default);

    string GetPullRequestUrl(PullRequest pullRequest);

    string GetBuildUrl(Build build);
}
