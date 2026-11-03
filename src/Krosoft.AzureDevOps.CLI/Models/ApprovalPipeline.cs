using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

/// <summary>
/// Pipeline concerné par une validation, avec le run qui attend.
/// </summary>
internal record ApprovalPipeline(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("owner")] ApprovalRun Owner);
