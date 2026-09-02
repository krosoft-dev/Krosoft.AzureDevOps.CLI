using System.Text.Json;
using System.Text.Json.Serialization;
using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Helpers;

internal static class ProfileLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    internal static async Task<(Profile? profile, string? error)> LoadAsync(string path)
    {
        if (!File.Exists(path))
        {
            return (null, $"Profil introuvable : {path}");
        }

        try
        {
            var json = await File.ReadAllTextAsync(path);
            var profile = JsonSerializer.Deserialize<Profile>(json, Options);
            if (profile is null)
            {
                return (null, "Profil invalide ou vide.");
            }

            var validationError = Validate(profile);
            return validationError is null
                ? (profile, null)
                : (null, validationError);
        }
        catch (Exception ex)
        {
            return (null, $"Erreur de lecture du profil : {ex.Message}");
        }
    }

    private static string? Validate(Profile profile)
    {
        if (profile.AzureDevOps is null)
        {
            return "Profil invalide : la section 'azureDevOps' est requise.";
        }

        if (string.IsNullOrWhiteSpace(profile.AzureDevOps.OrganizationUrl))
        {
            return "Profil invalide : le champ 'azureDevOps.organizationUrl' est requis (ex: https://dev.azure.com/mon-organisation).";
        }

        if (!Uri.TryCreate(profile.AzureDevOps.OrganizationUrl, UriKind.Absolute, out _))
        {
            return $"Profil invalide : 'azureDevOps.organizationUrl' n'est pas une URL valide ({profile.AzureDevOps.OrganizationUrl}).";
        }

        if (string.IsNullOrWhiteSpace(profile.AzureDevOps.Pat))
        {
            return "Profil invalide : le champ 'azureDevOps.pat' (Personal Access Token) est requis.";
        }

        return null;
    }
}
