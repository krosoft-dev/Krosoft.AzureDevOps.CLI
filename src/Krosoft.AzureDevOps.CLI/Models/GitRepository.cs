using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record GitRepository(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("project")] TeamProject Project);
