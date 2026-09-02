using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record PullRequestFilters(
    [property: JsonPropertyName("status")] PullRequestStatus Status = PullRequestStatus.Active,
    [property: JsonPropertyName("titles")] List<string>? Titles = null,
    [property: JsonPropertyName("exactTitle")]
    bool ExactTitle = false,
    [property: JsonPropertyName("repositories")]
    List<string>? Repositories = null)
{
    internal static readonly PullRequestFilters Default = new();
}
