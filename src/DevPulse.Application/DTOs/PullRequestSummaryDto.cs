namespace DevPulse.Application.DTOs;

public class PullRequestSummaryDto
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public bool IsMerged { get; set; }
    public bool IsDraft { get; set; }
    public string AuthorLogin { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? MergedAt { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
}
