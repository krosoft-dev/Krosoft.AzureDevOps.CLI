# Krosoft.AzureDevOps.CLI

[![forthebadge](https://forthebadge.com/badges/built-with-love.svg)](https://forthebadge.com) [![forthebadge](https://forthebadge.com/badges/made-with-c-sharp.svg)](https://forthebadge.com)

Outil CLI pour gérer en masse les pull requests d'une organisation Azure DevOps : lister, approuver, relancer les builds.

## Installation

```bash
dotnet pack .
dotnet tool install --global --add-source ./publish Krosoft.AzureDevOps.CLI
```

La commande installée s'appelle `krosoft-devops`. Le script `tools/scripts/dotnet_install_cli.ps1` enchaîne build, pack et réinstallation. Sans installer, chaque commande se lance aussi via `dotnet run` (voir plus bas).

## Profil

Toutes les commandes prennent `--profile <fichier.json>`. Le profil cible une organisation et définit les PR concernées.

```json
{
  "name": "tenor",
  "azureDevOps": {
    "organizationUrl": "https://dev.azure.com/mon-organisation",
    "pat": "xxxxxxxx",
    "projects": []
  },
  "pullRequests": {
    "status": "active",
    "titles": ["Renovate - Update all Krosoft.Extensions packages"],
    "exactTitle": true,
    "repositories": []
  }
}
```

| Champ | Description |
|-------|-------------|
| `pat` | Personal Access Token. Scopes : `Code (Read & Write)` et `Project and Team (Read)`. |
| `projects` | Projets à parcourir. Vide = tous les projets de l'organisation. |
| `status` | `active` (défaut), `completed`, `abandoned` ou `all`. |
| `titles` | Titres recherchés (OU, insensible à la casse). Vide = tous. |
| `exactTitle` | `true` : titre égal. `false` : titre contenant. |
| `repositories` | Dépôts concernés. Vide = tous. |

`files/local.json` est un exemple versionné. Les autres `files/*.json` sont ignorés par git : y mettre les profils avec un vrai PAT.

## Commandes

| Commande | Rôle |
|----------|------|
| `pr-list` | Liste les PR correspondant au profil. |
| `pr-approve` | Approuve les PR (les PR déjà approuvées sont ignorées). |
| `pr-requeue` | Relance les builds en échec (équivalent du bouton *Re-queue*). Les autres policies en échec sont affichées mais pas relançables. |

`pr-approve` et `pr-requeue` acceptent :

| Option | Description |
|--------|-------------|
| `--dry-run`, `-d` | Affiche ce qui serait fait, sans rien modifier. |
| `--ids`, `-i` | Limite aux PR indiquées (`--ids 6571,6572`). Remplace les filtres `titles`/`repositories`. |

Avec l'outil installé :

```bash
krosoft-devops pr-list --profile ./files/tenor.json
krosoft-devops pr-approve --profile ./files/tenor.json --dry-run
krosoft-devops pr-approve --profile ./files/tenor.json
krosoft-devops pr-requeue --profile ./files/tenor.json --ids 6562
```

Depuis les sources (le `--` sépare les arguments de `dotnet run` de ceux du CLI) :

```bash
dotnet run --project src/Krosoft.AzureDevOps.CLI -- pr-list --profile ./files/tenor.json
dotnet run --project src/Krosoft.AzureDevOps.CLI -- pr-approve --profile ./files/tenor.json --dry-run
dotnet run --project src/Krosoft.AzureDevOps.CLI -- pr-approve --profile ./files/tenor.json
dotnet run --project src/Krosoft.AzureDevOps.CLI -- pr-requeue --profile ./files/tenor.json --ids 6562
```

Code de sortie : `0` si tout s'est bien passé, `-1` en cas d'erreur ou si au moins une action a échoué.

> Une PR en *Auto-complete* est fusionnée dès que ses conditions sont remplies : vérifier en `--dry-run` avant d'approuver.
