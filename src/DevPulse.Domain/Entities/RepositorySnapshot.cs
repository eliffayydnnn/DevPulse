namespace DevPulse.Domain.Entities;

public class RepositorySnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime AnalyzedAtUtc { get; set; } = DateTime.UtcNow;
    public int Stars { get; set; }
    public int Forks { get; set; }
    public int OpenIssues { get; set; }
    public string? PrimaryLanguage { get; set; }
    public string SummaryMetricsJson { get; set; } = string.Empty;
}
