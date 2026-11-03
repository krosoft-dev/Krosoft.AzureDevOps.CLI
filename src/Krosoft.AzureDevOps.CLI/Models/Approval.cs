using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

/// <summary>
/// Validation manuelle (check "Approvals") posée sur un environnement ou une ressource d'un pipeline YAML.
/// </summary>
internal record Approval(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("createdOn")]
    DateTimeOffset CreatedOn,
    [property: JsonPropertyName("instructions")]
    string? Instructions,
    [property: JsonPropertyName("minRequiredApprovers")]
    int MinRequiredApprovers,
    [property: JsonPropertyName("steps")] List<ApprovalStep>? Steps,
    [property: JsonPropertyName("pipeline")]
    ApprovalPipeline Pipeline)
{
    /// <summary>
    /// Approbateurs dont la réponse est encore attendue, séparés par des virgules ("-" si aucun).
    /// </summary>
    internal string PendingApprovers
    {
        get
        {
            var names = (Steps ?? [])
                        .Where(s => string.Equals(s.Status, "pending", StringComparison.OrdinalIgnoreCase))
                        .Select(s => s.AssignedApprover.DisplayName)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

            return names.Count > 0 ? string.Join(", ", names) : "-";
        }
    }
}
