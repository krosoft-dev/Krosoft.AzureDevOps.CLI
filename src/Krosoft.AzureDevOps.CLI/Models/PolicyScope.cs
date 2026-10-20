using System.Text.Json.Serialization;

namespace Krosoft.AzureDevOps.CLI.Models;

// Portée d'une policy : un dépôt (ou tous les dépôts du projet si RepositoryId est null) et une branche.
// MatchKind : Exact (branche précise), Prefix (dossier de branches), DefaultBranch (branche par défaut de chaque dépôt).
internal record PolicyScope(
    [property: JsonPropertyName("repositoryId")]
    string? RepositoryId,
    [property: JsonPropertyName("refName")]
    string? RefName,
    [property: JsonPropertyName("matchKind")]
    string? MatchKind)
{
    internal bool IsProjectWide => string.IsNullOrEmpty(RepositoryId);

    internal string BranchLabel
    {
        get
        {
            if (string.Equals(MatchKind, "DefaultBranch", StringComparison.OrdinalIgnoreCase))
            {
                return "(branche par défaut)";
            }

            if (string.IsNullOrEmpty(RefName))
            {
                return "(toutes)";
            }

            var branch = RefName.StartsWith("refs/heads/", StringComparison.OrdinalIgnoreCase)
                ? RefName["refs/heads/".Length..]
                : RefName;

            return string.Equals(MatchKind, "Prefix", StringComparison.OrdinalIgnoreCase)
                ? $"{branch.TrimEnd('/')}/*"
                : branch;
        }
    }
}
