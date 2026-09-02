using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record TeamProject(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name);
