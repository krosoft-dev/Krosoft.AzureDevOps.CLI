using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record PolicyConfiguration(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("type")] PolicyType Type,
    [property: JsonPropertyName("isEnabled")]
    bool IsEnabled,
    [property: JsonPropertyName("isBlocking")]
    bool IsBlocking,
    [property: JsonPropertyName("settings")]
    PolicySettings? Settings);
