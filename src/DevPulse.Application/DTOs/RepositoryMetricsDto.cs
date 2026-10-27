namespace DevPulse.Application.DTOs;

public class RepositoryMetricsDto
{
    public int Stars { get; set; }
    public int Forks { get; set; }
    public int OpenIssues { get; set; }
    public int TotalRecentCommitsEvaluated { get; set; }
    public int OpenPullRequests { get; set; }
    public int ClosedPullRequests { get; set; }
    public int OpenIssuesCount { get; set; }
    public int ClosedIssuesCount { get; set; }
    public int HealthScore { get; set; } // 0 - 100
    public string HealthGrade { get; set; } = "A"; // "A+", "A", "B", "C", "D"
    public int DaysSinceLastCommit { get; set; }
    public double PullRequestMergeRatio { get; set; } // e.g. 0.85 (85%)
    public string ActivityStatus { get; set; } = "Active"; // "Very Active", "Active", "Moderate", "Dormant"
}
