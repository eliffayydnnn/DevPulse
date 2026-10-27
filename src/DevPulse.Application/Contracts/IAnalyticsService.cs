using DevPulse.Application.DTOs;

namespace DevPulse.Application.Contracts;

public interface IAnalyticsService
{
    Task<RepositoryAnalysisDto> AnalyzeRepositoryAsync(string owner, string repo, CancellationToken cancellationToken = default);
    Task<RepositoryComparisonDto> CompareRepositoriesAsync(IReadOnlyList<(string Owner, string Repo)> repos, CancellationToken cancellationToken = default);
}
