namespace DevPulse.Application.DTOs;

public class RepositoryAnalysisDto
{
    public GitHubRepositoryDto Repository { get; set; } = new();
    public RepositoryMetricsDto Metrics { get; set; } = new();
    public IReadOnlyList<CommitSummaryDto> RecentCommits { get; set; } = Array.Empty<CommitSummaryDto>();
    public IReadOnlyList<PullRequestSummaryDto> PullRequests { get; set; } = Array.Empty<PullRequestSummaryDto>();
    public IReadOnlyList<IssueSummaryDto> Issues { get; set; } = Array.Empty<IssueSummaryDto>();
    public IReadOnlyList<LanguageDistributionDto> Languages { get; set; } = Array.Empty<LanguageDistributionDto>();
    public IReadOnlyList<ActivityDataPointDto> ActivityTrend { get; set; } = Array.Empty<ActivityDataPointDto>();
    public DateTime AnalyzedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsFromCache { get; set; }
}
