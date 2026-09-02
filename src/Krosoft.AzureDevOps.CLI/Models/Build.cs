using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

// Statuts possibles : notStarted, inProgress, postponed, cancelling, completed.
internal record Build(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("buildNumber")]
    string BuildNumber,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("queueTime")]
    DateTimeOffset QueueTime,
    [property: JsonPropertyName("startTime")]
    DateTimeOffset? StartTime,
    [property: JsonPropertyName("sourceBranch")]
    string SourceBranch,
    [property: JsonPropertyName("definition")]
    BuildDefinitionReference Definition,
    [property: JsonPropertyName("project")] TeamProject Project,
    [property: JsonPropertyName("requestedFor")]
    IdentityRef? RequestedFor,
    [property: JsonPropertyName("triggerInfo")]
    Dictionary<string, string>? TriggerInfo)
{
    // Numéro de la PR à l'origine du build, si le build a été déclenché par une policy Build.
    internal string? PullRequestNumber =>
        TriggerInfo is not null && TriggerInfo.TryGetValue("pr.number", out var number) ? number : null;

    // Branche courte : refs/heads/main -> main, refs/pull/6562/merge -> pull/6562/merge.
    internal string ShortBranch =>
        SourceBranch.StartsWith("refs/heads/", StringComparison.Ordinal)
            ? SourceBranch["refs/heads/".Length..]
            : SourceBranch.StartsWith("refs/", StringComparison.Ordinal)
                ? SourceBranch["refs/".Length..]
                : SourceBranch;
}
