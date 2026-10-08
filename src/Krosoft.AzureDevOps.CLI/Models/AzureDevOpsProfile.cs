using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

internal record AzureDevOpsProfile(
    [property: JsonPropertyName("organizationUrl")]
    string OrganizationUrl,
    [property: JsonPropertyName("pat")] string Pat,
    [property: JsonPropertyName("projects")]
    List<string>? Projects = null,
    [property: JsonPropertyName("excludedProjects")]
    List<string>? ExcludedProjects = null);
