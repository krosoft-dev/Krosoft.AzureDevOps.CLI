using CommandLine;

namespace Krosoft.AzureDevOps.CLI;

internal static class Options
{
    [Verb("pr-list", HelpText = "Liste les pull requests d'une organisation Azure DevOps selon les filtres du profil.")]
    internal class PullRequestsOptions
    {
        [Option('p', "profile", Required = true, HelpText = "Chemin vers le fichier de profil JSON.")]
        public string Profile { get; set; } = string.Empty;
    }

    [Verb("pr-approve", HelpText = "Approuve toutes les pull requests correspondant aux filtres du profil.")]
    internal class ApproveOptions
    {
        [Option('p', "profile", Required = true, HelpText = "Chemin vers le fichier de profil JSON.")]
        public string Profile { get; set; } = string.Empty;

        [Option('d', "dry-run", Required = false, Default = false, HelpText = "Affiche les pull requests qui seraient approuvées, sans rien modifier.")]
        public bool DryRun { get; set; }

        [Option('i', "ids", Required = false, Separator = ',', HelpText = "Limite l'approbation à ces IDs de pull request (ex: --ids 6571,6572). Remplace les filtres titre/dépôt du profil.")]
        public IEnumerable<int> Ids { get; set; } = [];
    }

    [Verb("build-list", HelpText = "Liste les builds en cours et en attente des projets du profil.")]
    internal class BuildListOptions
    {
        [Option('p', "profile", Required = true, HelpText = "Chemin vers le fichier de profil JSON.")]
        public string Profile { get; set; } = string.Empty;
    }

    [Verb("pr-requeue", HelpText = "Relance les builds en échec (policies Build) des pull requests correspondant aux filtres du profil.")]
    internal class RequeueOptions
    {
        [Option('p', "profile", Required = true, HelpText = "Chemin vers le fichier de profil JSON.")]
        public string Profile { get; set; } = string.Empty;

        [Option('d', "dry-run", Required = false, Default = false, HelpText = "Affiche les builds qui seraient relancés, sans rien modifier.")]
        public bool DryRun { get; set; }

        [Option('i', "ids", Required = false, Separator = ',', HelpText = "Limite la relance à ces IDs de pull request (ex: --ids 6571,6572). Remplace les filtres titre/dépôt du profil.")]
        public IEnumerable<int> Ids { get; set; } = [];
    }
}
