namespace Krosoft.AzureDevOps.CLI.Models;

// Valeurs de vote de l'API Azure DevOps (champ "vote" d'un reviewer).
internal static class Vote
{
    internal const int Approved = 10;
    internal const int ApprovedWithSuggestions = 5;
    internal const int NoVote = 0;
    internal const int WaitingForAuthor = -5;
    internal const int Rejected = -10;

    internal static string ToLabel(int vote) => vote switch
    {
        Approved => "approuvée",
        ApprovedWithSuggestions => "approuvée avec suggestions",
        NoVote => "sans vote",
        WaitingForAuthor => "en attente de l'auteur",
        Rejected => "rejetée",
        _ => $"vote {vote}"
    };
}
