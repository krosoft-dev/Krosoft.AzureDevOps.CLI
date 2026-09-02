using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record IdentityRef([property: JsonPropertyName("displayName")] string DisplayName);
