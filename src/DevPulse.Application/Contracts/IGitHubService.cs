using DevPulse.Application.DTOs;
using DevPulse.Domain.Enums;

namespace DevPulse.Application.Contracts;

public interface IGitHubService
{
    Task<GitHubRepositoryDto> GetRepositoryAsync(string owner, string repo, CancellationToken cancellationToken = default);
    Task<GitHubUserDto> GetUserProfileAsync(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GitHubRepositoryDto>> GetUserRepositoriesAsync(string username, int limit = 30, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommitSummaryDto>> GetCommitsAsync(string owner, string repo, int limit = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PullRequestSummaryDto>> GetPullRequestsAsync(string owner, string repo, ItemStateFilter state = ItemStateFilter.All, int limit = 30, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IssueSummaryDto>> GetIssuesAsync(string owner, string repo, ItemStateFilter state = ItemStateFilter.All, int limit = 30, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LanguageDistributionDto>> GetLanguagesAsync(string owner, string repo, CancellationToken cancellationToken = default);
    Task<RateLimitInfoDto> GetRateLimitAsync(CancellationToken cancellationToken = default);
}
