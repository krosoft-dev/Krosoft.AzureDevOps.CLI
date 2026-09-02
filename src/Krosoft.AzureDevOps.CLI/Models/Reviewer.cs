using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record Reviewer(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("displayName")]
    string DisplayName,
    [property: JsonPropertyName("vote")] int Vote);
