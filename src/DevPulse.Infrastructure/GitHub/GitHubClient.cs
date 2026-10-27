using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using DevPulse.Application.Contracts;
using DevPulse.Application.DTOs;
using DevPulse.Domain.Enums;
using DevPulse.Domain.Exceptions;
using DevPulse.Infrastructure.GitHub.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DevPulse.Infrastructure.GitHub;

public class GitHubClient : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Standard GitHub Language Color Map for rich visual charts
    private static readonly Dictionary<string, string> LanguageColors = new(StringComparer.OrdinalIgnoreCase)
    {
        { "C#", "#178600" },
        { "TypeScript", "#3178c6" },
        { "JavaScript", "#f1e05a" },
        { "Python", "#3572A5" },
        { "Rust", "#dea584" },
        { "Go", "#00ADD8" },
        { "Java", "#b07219" },
        { "C++", "#f34b7d" },
        { "C", "#555555" },
        { "HTML", "#e34c26" },
        { "CSS", "#563d7c" },
        { "SCSS", "#c6538c" },
        { "PHP", "#4F5D95" },
        { "Ruby", "#701516" },
        { "Swift", "#F05138" },
        { "Kotlin", "#A97BFF" },
        { "Shell", "#89e051" },
        { "PowerShell", "#012456" },
        { "Docker", "#384d54" },
        { "Vue", "#41b883" }
    };

    public GitHubClient(HttpClient httpClient, IConfiguration configuration, ILogger<GitHubClient> _logger)
    {
        this._httpClient = httpClient;
        this._logger = _logger;

        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri("https://api.github.com/");
        }

        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DevPulse-Analytics", "1.0"));
        }

        if (!_httpClient.DefaultRequestHeaders.Accept.Any())
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        }

        var token = configuration["GitHub:Token"] ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(token) && _httpClient.DefaultRequestHeaders.Authorization == null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<GitHubRepositoryDto> GetRepositoryAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        var endpoint = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}";
        var res = await SendAndDeserializeAsync<GitHubApiRepoResponse>(endpoint, cancellationToken);

        return new GitHubRepositoryDto
        {
            Id = res.Id,
            Name = res.Name,
            FullName = res.FullName,
            Owner = res.Owner?.Login ?? owner,
            OwnerAvatarUrl = res.Owner?.AvatarUrl,
            Description = res.Description,
            HtmlUrl = res.HtmlUrl,
            Stars = res.StargazersCount,
            Forks = res.ForksCount,
            OpenIssues = res.OpenIssuesCount,
            Watchers = res.WatchersCount,
            PrimaryLanguage = res.Language,
            DefaultBranch = res.DefaultBranch,
            License = res.License?.Name ?? res.License?.SpdxId,
            IsArchived = res.Archived,
            IsFork = res.Fork,
            CreatedAt = res.CreatedAt,
            UpdatedAt = res.UpdatedAt,
            PushedAt = res.PushedAt
        };
    }

    public async Task<GitHubUserDto> GetUserProfileAsync(string username, CancellationToken cancellationToken = default)
    {
        var endpoint = $"users/{Uri.EscapeDataString(username)}";
        var res = await SendAndDeserializeAsync<GitHubApiUserResponse>(endpoint, cancellationToken);

        return new GitHubUserDto
        {
            Login = res.Login,
            Name = res.Name,
            AvatarUrl = res.AvatarUrl,
            Bio = res.Bio,
            Company = res.Company,
            Location = res.Location,
            Blog = res.Blog,
            TwitterUsername = res.TwitterUsername,
            PublicRepos = res.PublicRepos,
            Followers = res.Followers,
            Following = res.Following,
            HtmlUrl = res.HtmlUrl,
            CreatedAt = res.CreatedAt
        };
    }

    public async Task<IReadOnlyList<GitHubRepositoryDto>> GetUserRepositoriesAsync(string username, int limit = 30, CancellationToken cancellationToken = default)
    {
        var endpoint = $"users/{Uri.EscapeDataString(username)}/repos?per_page={Math.Clamp(limit, 1, 100)}&sort=updated";
        var list = await SendAndDeserializeAsync<List<GitHubApiRepoResponse>>(endpoint, cancellationToken);

        return list.Select(res => new GitHubRepositoryDto
        {
            Id = res.Id,
            Name = res.Name,
            FullName = res.FullName,
            Owner = res.Owner?.Login ?? username,
            OwnerAvatarUrl = res.Owner?.AvatarUrl,
            Description = res.Description,
            HtmlUrl = res.HtmlUrl,
            Stars = res.StargazersCount,
            Forks = res.ForksCount,
            OpenIssues = res.OpenIssuesCount,
            Watchers = res.WatchersCount,
            PrimaryLanguage = res.Language,
            DefaultBranch = res.DefaultBranch,
            License = res.License?.Name,
            IsArchived = res.Archived,
            IsFork = res.Fork,
            CreatedAt = res.CreatedAt,
            UpdatedAt = res.UpdatedAt,
            PushedAt = res.PushedAt
        }).ToList();
    }

    public async Task<IReadOnlyList<CommitSummaryDto>> GetCommitsAsync(string owner, string repo, int limit = 50, CancellationToken cancellationToken = default)
    {
        var endpoint = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/commits?per_page={Math.Clamp(limit, 1, 100)}";
        try
        {
            var list = await SendAndDeserializeAsync<List<GitHubApiCommitResponse>>(endpoint, cancellationToken);
            return list.Select(c => new CommitSummaryDto
            {
                Sha = c.Sha,
                Message = c.Commit?.Message?.Split('\n').FirstOrDefault() ?? string.Empty,
                AuthorName = c.Commit?.Author?.Name ?? "Unknown",
                AuthorEmail = c.Commit?.Author?.Email,
                AuthorAvatarUrl = c.Author?.AvatarUrl,
                Date = c.Commit?.Author?.Date ?? DateTime.UtcNow,
                HtmlUrl = c.HtmlUrl
            }).ToList();
        }
        catch (NotFoundException)
        {
            // Empty repo with 0 commits can return 409 or 404 in GitHub API
            return Array.Empty<CommitSummaryDto>();
        }
    }

    public async Task<IReadOnlyList<PullRequestSummaryDto>> GetPullRequestsAsync(string owner, string repo, ItemStateFilter state = ItemStateFilter.All, int limit = 30, CancellationToken cancellationToken = default)
    {
        var stateQuery = state switch
        {
            ItemStateFilter.Open => "open",
            ItemStateFilter.Closed => "closed",
            _ => "all"
        };

        var endpoint = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/pulls?state={stateQuery}&per_page={Math.Clamp(limit, 1, 100)}&sort=updated&direction=desc";
        var list = await SendAndDeserializeAsync<List<GitHubApiPullRequestResponse>>(endpoint, cancellationToken);

        return list.Select(pr => new PullRequestSummaryDto
        {
            Number = pr.Number,
            Title = pr.Title,
            State = pr.State,
            IsMerged = pr.MergedAt.HasValue,
            IsDraft = pr.Draft,
            AuthorLogin = pr.User?.Login ?? "Unknown",
            AuthorAvatarUrl = pr.User?.AvatarUrl,
            CreatedAt = pr.CreatedAt,
            UpdatedAt = pr.UpdatedAt,
            ClosedAt = pr.ClosedAt,
            MergedAt = pr.MergedAt,
            HtmlUrl = pr.HtmlUrl
        }).ToList();
    }

    public async Task<IReadOnlyList<IssueSummaryDto>> GetIssuesAsync(string owner, string repo, ItemStateFilter state = ItemStateFilter.All, int limit = 30, CancellationToken cancellationToken = default)
    {
        var stateQuery = state switch
        {
            ItemStateFilter.Open => "open",
            ItemStateFilter.Closed => "closed",
            _ => "all"
        };

        // Note: GitHub's /issues endpoint returns both issues and pull requests unless filtered out
        var endpoint = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/issues?state={stateQuery}&per_page={Math.Clamp(limit, 1, 100)}&sort=updated&direction=desc";
        var list = await SendAndDeserializeAsync<List<GitHubApiIssueResponse>>(endpoint, cancellationToken);

        // Filter out pull requests (they have pull_request key populated)
        return list.Where(i => i.PullRequest == null).Select(i => new IssueSummaryDto
        {
            Number = i.Number,
            Title = i.Title,
            State = i.State,
            AuthorLogin = i.User?.Login ?? "Unknown",
            AuthorAvatarUrl = i.User?.AvatarUrl,
            CommentsCount = i.Comments,
            Labels = i.Labels?.Select(l => l.Name).ToList() ?? new List<string>(),
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt,
            ClosedAt = i.ClosedAt,
            HtmlUrl = i.HtmlUrl
        }).ToList();
    }

    public async Task<IReadOnlyList<LanguageDistributionDto>> GetLanguagesAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        var endpoint = $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repo)}/languages";
        var dict = await SendAndDeserializeAsync<Dictionary<string, long>>(endpoint, cancellationToken);

        var totalBytes = dict.Values.Sum();
        if (totalBytes == 0)
        {
            return Array.Empty<LanguageDistributionDto>();
        }

        return dict.Select(pair => new LanguageDistributionDto
        {
            Language = pair.Key,
            Bytes = pair.Value,
            Percentage = Math.Round((double)pair.Value / totalBytes * 100, 1),
            ColorHex = LanguageColors.TryGetValue(pair.Key, out var color) ? color : "#8b949e"
        }).OrderByDescending(l => l.Bytes).ToList();
    }

    public async Task<RateLimitInfoDto> GetRateLimitAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = "rate_limit";
        var res = await SendAndDeserializeAsync<GitHubApiRateLimitResponse>(endpoint, cancellationToken);
        var core = res.Resources?.Core;

        return new RateLimitInfoDto
        {
            Limit = core?.Limit ?? 60,
            Remaining = core?.Remaining ?? 60,
            ResetTimeUtc = core != null ? DateTimeOffset.FromUnixTimeSeconds(core.Reset).UtcDateTime : null
        };
    }

    private async Task<T> SendAndDeserializeAsync<T>(string endpoint, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);

        ExtractRateLimits(response);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException("GitHub resource", endpoint);
        }

        if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == (HttpStatusCode)429)
        {
            var resetTime = TryGetRateLimitReset(response);
            var remaining = TryGetRateLimitRemaining(response);
            if (remaining.HasValue && remaining.Value == 0)
            {
                throw new RateLimitExceededException("GitHub API rate limit exceeded. Please wait until the reset window expires.", resetTime, remaining);
            }

            throw new DomainException($"GitHub API access denied or forbidden for endpoint: {endpoint}.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("GitHub API non-success code {StatusCode} for {Endpoint}: {ErrorBody}", response.StatusCode, endpoint, errorBody);
            throw new DomainException($"GitHub API returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) for {endpoint}.");
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);

        if (result == null)
        {
            throw new DomainException($"Failed to deserialize response from GitHub API for endpoint: {endpoint}.");
        }

        return result;
    }

    private void ExtractRateLimits(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("x-ratelimit-remaining", out var remVals) &&
            int.TryParse(remVals.FirstOrDefault(), out var remaining))
        {
            if (remaining <= 5)
            {
                _logger.LogWarning("GitHub API remaining quota is low: {Remaining} requests left.", remaining);
            }
        }
    }

    private static DateTime? TryGetRateLimitReset(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("x-ratelimit-reset", out var resetVals) &&
            long.TryParse(resetVals.FirstOrDefault(), out var epochSeconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(epochSeconds).UtcDateTime;
        }
        return null;
    }

    private static int? TryGetRateLimitRemaining(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("x-ratelimit-remaining", out var remVals) &&
            int.TryParse(remVals.FirstOrDefault(), out var remaining))
        {
            return remaining;
        }
        return null;
    }
}
