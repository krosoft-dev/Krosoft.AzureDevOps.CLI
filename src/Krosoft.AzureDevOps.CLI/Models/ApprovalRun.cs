using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

/// <summary>
/// Run (build) bloqué par une validation : <c>id</c> est l'ID du build, <c>name</c> son numéro.
/// </summary>
internal record ApprovalRun(
    [property: JsonPropertyName("id")]
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    int Id,
    [property: JsonPropertyName("name")] string Name);
