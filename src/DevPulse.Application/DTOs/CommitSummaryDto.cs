namespace DevPulse.Application.DTOs;

public class CommitSummaryDto
{
    public string Sha { get; set; } = string.Empty;
    public string ShortSha => Sha.Length >= 7 ? Sha[..7] : Sha;
    public string Message { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorEmail { get; set; }
    public string? AuthorAvatarUrl { get; set; }
    public DateTime Date { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
}
