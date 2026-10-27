namespace DevPulse.Application.DTOs;

public class MetricComparisonRowDto
{
    public string MetricKey { get; set; } = string.Empty;
    public string MetricLabel { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Popularity", "Activity", "Health", "Maintenance"
    public Dictionary<string, string> FormattedValues { get; set; } = new();
    public string? WinnerRepositoryFullName { get; set; }
}

public class RepositoryComparisonDto
{
    public IReadOnlyList<RepositoryAnalysisDto> Repositories { get; set; } = Array.Empty<RepositoryAnalysisDto>();
    public IReadOnlyList<MetricComparisonRowDto> ComparisonMatrix { get; set; } = Array.Empty<MetricComparisonRowDto>();
    public string OverallVerdict { get; set; } = string.Empty;
    public DateTime ComparedAtUtc { get; set; } = DateTime.UtcNow;
}
