using System.Globalization;
using System.Text.Json;
using DevPulse.Application.Contracts;
using DevPulse.Application.DTOs;
using DevPulse.Domain.Entities;
using DevPulse.Domain.Enums;
using DevPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevPulse.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IGitHubService _gitHubService;
    private readonly ICacheService _cacheService;
    private readonly DevPulseDbContext _dbContext;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IGitHubService gitHubService,
        ICacheService cacheService,
        DevPulseDbContext dbContext,
        ILogger<AnalyticsService> logger)
    {
        _gitHubService = gitHubService;
        _cacheService = cacheService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<RepositoryAnalysisDto> AnalyzeRepositoryAsync(string owner, string repo, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"repo_analysis_{owner.ToLowerInvariant()}_{repo.ToLowerInvariant()}";
        var cached = await _cacheService.GetAsync<RepositoryAnalysisDto>(cacheKey, cancellationToken);
        if (cached != null)
        {
            cached.IsFromCache = true;
            return cached;
        }

        // Fetch repo, commits, PRs, issues, languages in parallel
        var repoTask = _gitHubService.GetRepositoryAsync(owner, repo, cancellationToken);
        var commitsTask = _gitHubService.GetCommitsAsync(owner, repo, 50, cancellationToken);
        var prsTask = _gitHubService.GetPullRequestsAsync(owner, repo, ItemStateFilter.All, 30, cancellationToken);
        var issuesTask = _gitHubService.GetIssuesAsync(owner, repo, ItemStateFilter.All, 30, cancellationToken);
        var languagesTask = _gitHubService.GetLanguagesAsync(owner, repo, cancellationToken);

        await Task.WhenAll(repoTask, commitsTask, prsTask, issuesTask, languagesTask);

        var repository = await repoTask;
        var commits = await commitsTask;
        var prs = await prsTask;
        var issues = await issuesTask;
        var languages = await languagesTask;

        // Calculate metrics
        var metrics = ComputeMetrics(repository, commits, prs, issues);

        // Generate activity trend (last 6 periods / weeks)
        var trend = ComputeActivityTrend(commits, prs, issues);

        var analysis = new RepositoryAnalysisDto
        {
            Repository = repository,
            Metrics = metrics,
            RecentCommits = commits,
            PullRequests = prs,
            Issues = issues,
            Languages = languages,
            ActivityTrend = trend,
            AnalyzedAtUtc = DateTime.UtcNow,
            IsFromCache = false
        };

        // Cache the computed result for 10 minutes
        await _cacheService.SetAsync(cacheKey, analysis, TimeSpan.FromMinutes(10), cancellationToken);

        // Record snapshot in database asynchronously (swallow DB errors if offline)
        _ = RecordSnapshotAsync(repository, metrics, cancellationToken);

        return analysis;
    }

    public async Task<RepositoryComparisonDto> CompareRepositoriesAsync(
        IReadOnlyList<(string Owner, string Repo)> repos,
        CancellationToken cancellationToken = default)
    {
        var tasks = repos.Select(r => AnalyzeRepositoryAsync(r.Owner, r.Repo, cancellationToken)).ToList();
        var analyses = (await Task.WhenAll(tasks)).ToList();

        var matrix = new List<MetricComparisonRowDto>();

        // 1. Stars
        matrix.Add(BuildComparisonRow(
            "Stars",
            "Star Count",
            "Popularity",
            analyses,
            a => a.Repository.Stars.ToString("N0", CultureInfo.InvariantCulture),
            analyses.OrderByDescending(a => a.Repository.Stars).First().Repository.FullName));

        // 2. Forks
        matrix.Add(BuildComparisonRow(
            "Forks",
            "Fork Count",
            "Popularity",
            analyses,
            a => a.Repository.Forks.ToString("N0", CultureInfo.InvariantCulture),
            analyses.OrderByDescending(a => a.Repository.Forks).First().Repository.FullName));

        // 3. Health Score
        matrix.Add(BuildComparisonRow(
            "HealthScore",
            "Health Score (0-100)",
            "Health",
            analyses,
            a => $"{a.Metrics.HealthScore}/100 ({a.Metrics.HealthGrade})",
            analyses.OrderByDescending(a => a.Metrics.HealthScore).First().Repository.FullName));

        // 4. Open Issues
        matrix.Add(BuildComparisonRow(
            "OpenIssues",
            "Open Issues",
            "Maintenance",
            analyses,
            a => a.Repository.OpenIssues.ToString("N0", CultureInfo.InvariantCulture),
            analyses.OrderBy(a => a.Repository.OpenIssues).First().Repository.FullName));

        // 5. Activity Status
        matrix.Add(BuildComparisonRow(
            "ActivityStatus",
            "Activity Level",
            "Activity",
            analyses,
            a => a.Metrics.ActivityStatus,
            analyses.OrderBy(a => a.Metrics.DaysSinceLastCommit).First().Repository.FullName));

        // 6. Days Since Last Commit
        matrix.Add(BuildComparisonRow(
            "DaysSinceLastCommit",
            "Days Since Last Commit",
            "Activity",
            analyses,
            a => $"{a.Metrics.DaysSinceLastCommit} days",
            analyses.OrderBy(a => a.Metrics.DaysSinceLastCommit).First().Repository.FullName));

        // 7. PR Merge Ratio
        matrix.Add(BuildComparisonRow(
            "PRMergeRatio",
            "PR Merge Ratio",
            "Maintenance",
            analyses,
            a => $"{Math.Round(a.Metrics.PullRequestMergeRatio * 100, 1)}%",
            analyses.OrderByDescending(a => a.Metrics.PullRequestMergeRatio).First().Repository.FullName));

        // 8. Primary Language
        matrix.Add(BuildComparisonRow(
            "PrimaryLanguage",
            "Primary Language",
            "Stack",
            analyses,
            a => a.Repository.PrimaryLanguage ?? "N/A",
            null));

        // Determine overall verdict
        var topHealth = analyses.OrderByDescending(a => a.Metrics.HealthScore).First();
        var topStars = analyses.OrderByDescending(a => a.Repository.Stars).First();

        var verdict = topHealth.Repository.FullName == topStars.Repository.FullName
            ? $"{topHealth.Repository.FullName} is the clear frontrunner with the highest community popularity ({topHealth.Repository.Stars:N0} stars) and the top health grade ({topHealth.Metrics.HealthGrade}, {topHealth.Metrics.HealthScore}/100)."
            : $"{topStars.Repository.FullName} leads in total popularity ({topStars.Repository.Stars:N0} stars), while {topHealth.Repository.FullName} exhibits superior engineering health ({topHealth.Metrics.HealthScore}/100) and recent commit velocity.";

        return new RepositoryComparisonDto
        {
            Repositories = analyses,
            ComparisonMatrix = matrix,
            OverallVerdict = verdict,
            ComparedAtUtc = DateTime.UtcNow
        };
    }

    private static RepositoryMetricsDto ComputeMetrics(
        GitHubRepositoryDto repo,
        IReadOnlyList<CommitSummaryDto> commits,
        IReadOnlyList<PullRequestSummaryDto> prs,
        IReadOnlyList<IssueSummaryDto> issues)
    {
        var openPrs = prs.Count(p => p.State.Equals("open", StringComparison.OrdinalIgnoreCase));
        var closedPrs = prs.Count(p => p.State.Equals("closed", StringComparison.OrdinalIgnoreCase));
        var mergedPrs = prs.Count(p => p.IsMerged);

        var openIssues = issues.Count(i => i.State.Equals("open", StringComparison.OrdinalIgnoreCase));
        var closedIssues = issues.Count(i => i.State.Equals("closed", StringComparison.OrdinalIgnoreCase));

        var daysSinceCommit = 999;
        var lastCommit = commits.OrderByDescending(c => c.Date).FirstOrDefault();
        if (lastCommit != null)
        {
            daysSinceCommit = Math.Max(0, (int)(DateTime.UtcNow - lastCommit.Date).TotalDays);
        }
        else if (repo.PushedAt.HasValue)
        {
            daysSinceCommit = Math.Max(0, (int)(DateTime.UtcNow - repo.PushedAt.Value).TotalDays);
        }

        // Health Score calculation (0 - 100)
        var score = 0;

        // Recency of commits (30 pts max)
        if (daysSinceCommit <= 7) score += 30;
        else if (daysSinceCommit <= 30) score += 20;
        else if (daysSinceCommit <= 90) score += 10;

        // PR handling (20 pts max)
        var totalPrs = openPrs + closedPrs;
        if (totalPrs > 0)
        {
            var mergeOrCloseRatio = (double)closedPrs / totalPrs;
            score += (int)(mergeOrCloseRatio * 20);
        }
        else
        {
            score += 10;
        }

        // Issue backlog ratio (20 pts max)
        var totalIssues = openIssues + closedIssues;
        if (totalIssues > 0)
        {
            var closedRatio = (double)closedIssues / totalIssues;
            score += (int)(closedRatio * 20);
        }
        else
        {
            score += 15;
        }

        // Project hygiene & documentation
        if (!string.IsNullOrWhiteSpace(repo.Description)) score += 10;
        if (!string.IsNullOrWhiteSpace(repo.License)) score += 10;
        if (!repo.IsArchived) score += 10;

        score = Math.Clamp(score, 0, 100);

        var grade = score switch
        {
            >= 90 => "A+",
            >= 80 => "A",
            >= 70 => "B",
            >= 55 => "C",
            _ => "D"
        };

        var activityStatus = daysSinceCommit switch
        {
            <= 7 => "Very Active",
            <= 30 => "Active",
            <= 90 => "Moderate",
            _ => "Dormant"
        };

        var prRatio = totalPrs > 0 ? (double)mergedPrs / totalPrs : 0;

        return new RepositoryMetricsDto
        {
            Stars = repo.Stars,
            Forks = repo.Forks,
            OpenIssues = repo.OpenIssues,
            TotalRecentCommitsEvaluated = commits.Count,
            OpenPullRequests = openPrs,
            ClosedPullRequests = closedPrs,
            OpenIssuesCount = openIssues,
            ClosedIssuesCount = closedIssues,
            HealthScore = score,
            HealthGrade = grade,
            DaysSinceLastCommit = daysSinceCommit,
            PullRequestMergeRatio = prRatio,
            ActivityStatus = activityStatus
        };
    }

    private static IReadOnlyList<ActivityDataPointDto> ComputeActivityTrend(
        IReadOnlyList<CommitSummaryDto> commits,
        IReadOnlyList<PullRequestSummaryDto> prs,
        IReadOnlyList<IssueSummaryDto> issues)
    {
        var now = DateTime.UtcNow.Date;
        var points = new List<ActivityDataPointDto>();

        // Generate 6 weekly periods
        for (int i = 5; i >= 0; i--)
        {
            var start = now.AddDays(-(i + 1) * 7);
            var end = now.AddDays(-i * 7);

            var label = $"{start:MMM dd} - {end:MMM dd}";

            var cCount = commits.Count(c => c.Date >= start && c.Date < end);
            var prCount = prs.Count(p => p.CreatedAt >= start && p.CreatedAt < end);
            var issueCount = issues.Count(iss => iss.CreatedAt >= start && iss.CreatedAt < end);

            points.Add(new ActivityDataPointDto
            {
                PeriodLabel = label,
                PeriodStart = start,
                Commits = cCount,
                PullRequests = prCount,
                Issues = issueCount
            });
        }

        return points;
    }

    private static MetricComparisonRowDto BuildComparisonRow(
        string key,
        string label,
        string category,
        IReadOnlyList<RepositoryAnalysisDto> analyses,
        Func<RepositoryAnalysisDto, string> valueSelector,
        string? winnerRepo)
    {
        var dict = new Dictionary<string, string>();
        foreach (var a in analyses)
        {
            dict[a.Repository.FullName] = valueSelector(a);
        }

        return new MetricComparisonRowDto
        {
            MetricKey = key,
            MetricLabel = label,
            Category = category,
            FormattedValues = dict,
            WinnerRepositoryFullName = winnerRepo
        };
    }

    private async Task RecordSnapshotAsync(GitHubRepositoryDto repo, RepositoryMetricsDto metrics, CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = new RepositorySnapshot
            {
                Owner = repo.Owner,
                Name = repo.Name,
                Stars = repo.Stars,
                Forks = repo.Forks,
                OpenIssues = repo.OpenIssues,
                PrimaryLanguage = repo.PrimaryLanguage,
                AnalyzedAtUtc = DateTime.UtcNow,
                SummaryMetricsJson = JsonSerializer.Serialize(metrics)
            };

            _dbContext.RepositorySnapshots.Add(snapshot);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not persist snapshot to database (normal during offline/dev mode without active DB).");
        }
    }
}
