using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record PullRequest(
    [property: JsonPropertyName("pullRequestId")]
    int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("isDraft")] bool IsDraft,
    [property: JsonPropertyName("creationDate")]
    DateTimeOffset CreationDate,
    [property: JsonPropertyName("sourceRefName")]
    string SourceRefName,
    [property: JsonPropertyName("targetRefName")]
    string TargetRefName,
    [property: JsonPropertyName("createdBy")]
    IdentityRef CreatedBy,
    [property: JsonPropertyName("repository")]
    GitRepository Repository,
    [property: JsonPropertyName("reviewers")]
    List<Reviewer>? Reviewers = null,
    [property: JsonPropertyName("autoCompleteSetBy")]
    IdentityRef? AutoCompleteSetBy = null)
{
    // Azure DevOps ne renvoie autoCompleteSetBy que lorsque l'auto-complétion est activée.
    internal bool IsAutoCompleteSet => AutoCompleteSetBy is not null;

    internal int VoteOf(string reviewerId) =>
        Reviewers?.FirstOrDefault(r => string.Equals(r.Id, reviewerId, StringComparison.OrdinalIgnoreCase))?.Vote ?? Vote.NoVote;

    internal bool IsApprovedBy(string reviewerId) => VoteOf(reviewerId) >= Vote.ApprovedWithSuggestions;
}
