namespace DevPulse.Application.DTOs;

public class IssueSummaryDto
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string AuthorLogin { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public int CommentsCount { get; set; }
    public IReadOnlyList<string> Labels { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
}
