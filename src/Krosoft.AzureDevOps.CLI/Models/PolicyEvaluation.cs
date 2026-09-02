using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

// Statuts possibles : queued, running, approved, rejected, notApplicable, broken.
internal record PolicyEvaluation(
    [property: JsonPropertyName("evaluationId")]
    Guid EvaluationId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("configuration")]
    PolicyConfiguration Configuration)
{
    internal string Name => Configuration.Settings?.DisplayName is { Length: > 0 } name
        ? name
        : Configuration.Type.DisplayName;

    internal bool IsBuild =>
        string.Equals(Configuration.Type.Id, PolicyType.BuildTypeId, StringComparison.OrdinalIgnoreCase);

    internal bool IsFailed =>
        string.Equals(Status, "rejected", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Status, "broken", StringComparison.OrdinalIgnoreCase);

    // Seules les policies Build en échec peuvent être relancées (équivalent du bouton "Re-queue").
    internal bool CanRequeue => Configuration.IsEnabled && IsBuild && IsFailed;
}
