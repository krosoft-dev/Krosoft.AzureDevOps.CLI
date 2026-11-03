using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

/// <summary>
/// Étape d'une validation : un approbateur assigné et son statut (pending, approved, rejected...).
/// </summary>
internal record ApprovalStep(
    [property: JsonPropertyName("assignedApprover")]
    IdentityRef AssignedApprover,
    [property: JsonPropertyName("status")] string Status);
