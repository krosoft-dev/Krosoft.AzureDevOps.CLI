using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record BuildDefinitionReference(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name);
