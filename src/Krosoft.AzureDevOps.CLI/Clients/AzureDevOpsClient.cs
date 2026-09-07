using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Krosoft.AzureDevOps.CLI.Interfaces;
using Krosoft.AzureDevOps.CLI.Models;

namespace Krosoft.AzureDevOps.CLI.Clients;

internal sealed class AzureDevOpsClient : IAzureDevOpsClient
{
    private const string ApiVersion = "7.1";
    private const string PolicyApiVersion = "7.1-preview.1"; // L'API policy evaluations n'existe qu'en preview.
    private const int PageSize = 100;
    private const string ContinuationTokenHeader = "x-ms-continuationtoken";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;
    private readonly string _organizationUrl;

    public AzureDevOpsClient(AzureDevOpsProfile profile)
    {
        _organizationUrl = profile.OrganizationUrl.TrimEnd('/');

        _httpClient = new HttpClient();
        var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{profile.Pat}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<IReadOnlyList<TeamProject>> GetProjectsAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<TeamProject>();
        string? continuationToken = null;

        do
        {
            var url = $"{_organizationUrl}/_apis/projects?$top={PageSize}&api-version={ApiVersion}";
            if (continuationToken is not null)
            {
                url += $"&continuationToken={Uri.EscapeDataString(continuationToken)}";
            }

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);

            var page = await response.Content.ReadFromJsonAsync<ListResponse<TeamProject>>(JsonOptions, cancellationToken);
            if (page is not null)
            {
                result.AddRange(page.Value);
            }

            continuationToken = response.Headers.TryGetValues(ContinuationTokenHeader, out var values)
                ? values.FirstOrDefault()
                : null;
        } while (!string.IsNullOrEmpty(continuationToken));

        return result.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<IReadOnlyList<PullRequest>> GetPullRequestsAsync(string project, PullRequestStatus status, CancellationToken cancellationToken = default)
    {
        var result = new List<PullRequest>();
        var skip = 0;

        while (true)
        {
            var url = $"{_organizationUrl}/{Uri.EscapeDataString(project)}/_apis/git/pullrequests" +
                      $"?searchCriteria.status={status.ToString().ToLowerInvariant()}" +
                      $"&$top={PageSize}&$skip={skip}&api-version={ApiVersion}";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);

            var page = await response.Content.ReadFromJsonAsync<ListResponse<PullRequest>>(JsonOptions, cancellationToken);
            if (page is null || page.Value.Count == 0)
            {
                break;
            }

            result.AddRange(page.Value);
            if (page.Value.Count < PageSize)
            {
                break;
            }

            skip += PageSize;
        }

        return result;
    }

    public async Task<string> GetCurrentUserIdAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{_organizationUrl}/_apis/connectionData", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        if (doc.RootElement.TryGetProperty("authenticatedUser", out var user) &&
            user.TryGetProperty("id", out var id) &&
            id.GetString() is { Length: > 0 } userId)
        {
            return userId;
        }

        throw new InvalidOperationException("Impossible de déterminer l'utilisateur associé au PAT.");
    }

    public async Task ApproveAsync(PullRequest pullRequest, string reviewerId, CancellationToken cancellationToken = default)
    {
        var url = $"{_organizationUrl}/{Uri.EscapeDataString(pullRequest.Repository.Project.Name)}" +
                  $"/_apis/git/repositories/{pullRequest.Repository.Id}/pullRequests/{pullRequest.Id}" +
                  $"/reviewers/{reviewerId}?api-version={ApiVersion}";

        using var response = await _httpClient.PutAsJsonAsync(url, new { vote = Vote.Approved }, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    // Active l'auto-complétion : la PR est fusionnée automatiquement dès que les policies obligatoires sont satisfaites.
    // Sans completionOptions, Azure applique la stratégie de merge et le comportement de branche configurés sur le dépôt.
    public async Task SetAutoCompleteAsync(PullRequest pullRequest, string reviewerId, CancellationToken cancellationToken = default)
    {
        var url = $"{_organizationUrl}/{Uri.EscapeDataString(pullRequest.Repository.Project.Name)}" +
                  $"/_apis/git/repositories/{pullRequest.Repository.Id}/pullRequests/{pullRequest.Id}?api-version={ApiVersion}";

        using var response = await _httpClient.PatchAsJsonAsync(url, new { autoCompleteSetBy = new { id = reviewerId } }, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<PolicyEvaluation>> GetPolicyEvaluationsAsync(PullRequest pullRequest, CancellationToken cancellationToken = default)
    {
        var artifactId = $"vstfs:///CodeReview/CodeReviewId/{pullRequest.Repository.Project.Id}/{pullRequest.Id}";
        var url = $"{_organizationUrl}/{Uri.EscapeDataString(pullRequest.Repository.Project.Name)}/_apis/policy/evaluations" +
                  $"?artifactId={Uri.EscapeDataString(artifactId)}&api-version={PolicyApiVersion}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var page = await response.Content.ReadFromJsonAsync<ListResponse<PolicyEvaluation>>(JsonOptions, cancellationToken);
        return page?.Value ?? [];
    }

    public async Task RequeuePolicyEvaluationAsync(PullRequest pullRequest, Guid evaluationId, CancellationToken cancellationToken = default)
    {
        var url = $"{_organizationUrl}/{Uri.EscapeDataString(pullRequest.Repository.Project.Name)}/_apis/policy/evaluations/{evaluationId}" +
                  $"?api-version={PolicyApiVersion}";

        using var response = await _httpClient.PatchAsync(url, new StringContent(string.Empty, Encoding.UTF8, "application/json"), cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    // Builds en cours (inProgress) et en attente (notStarted).
    public async Task<IReadOnlyList<Build>> GetActiveBuildsAsync(string project, CancellationToken cancellationToken = default)
    {
        var url = $"{_organizationUrl}/{Uri.EscapeDataString(project)}/_apis/build/builds" +
                  $"?statusFilter=inProgress,notStarted&api-version={ApiVersion}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var page = await response.Content.ReadFromJsonAsync<ListResponse<Build>>(JsonOptions, cancellationToken);
        return page?.Value ?? [];
    }

    public string GetBuildUrl(Build build) =>
        $"{_organizationUrl}/{Uri.EscapeDataString(build.Project.Name)}/_build/results?buildId={build.Id}";

    public string GetPullRequestUrl(PullRequest pullRequest) =>
        $"{_organizationUrl}/{Uri.EscapeDataString(pullRequest.Repository.Project.Name)}/_git/{Uri.EscapeDataString(pullRequest.Repository.Name)}/pullrequest/{pullRequest.Id}";

    public void Dispose() => _httpClient.Dispose();

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            // Azure DevOps renvoie une page de login HTML (HTTP 200) quand le PAT est invalide ou expiré.
            if (response.Content.Headers.ContentType?.MediaType?.Contains("json", StringComparison.OrdinalIgnoreCase) != true)
            {
                throw new InvalidOperationException("Réponse non JSON reçue : le PAT est probablement invalide ou expiré.");
            }

            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var detail = TryExtractMessage(body) ?? body;
        throw new HttpRequestException($"Erreur HTTP {(int)response.StatusCode} ({response.StatusCode}) : {detail}");
    }

    private static string? TryExtractMessage(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.TryGetProperty("message", out var message) ? message.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
