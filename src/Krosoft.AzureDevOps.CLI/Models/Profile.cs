using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record Profile(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("azureDevOps")]
    AzureDevOpsProfile AzureDevOps,
    [property: JsonPropertyName("pullRequests")]
    PullRequestFilters? PullRequests = null);
