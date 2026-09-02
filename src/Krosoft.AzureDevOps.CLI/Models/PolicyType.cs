using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record PolicyType(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("displayName")]
    string DisplayName)
{
    // Identifiant fixe du type de policy "Build" dans Azure DevOps.
    internal const string BuildTypeId = "0609b952-1397-4640-95ec-e00a01b2c241";
}
