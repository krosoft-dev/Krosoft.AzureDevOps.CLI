using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record PolicySettings(
    [property: JsonPropertyName("displayName")]
    string? DisplayName,
    [property: JsonPropertyName("buildDefinitionId")]
    int? BuildDefinitionId);
