using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record ListResponse<T>(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("value")] List<T> Value);
