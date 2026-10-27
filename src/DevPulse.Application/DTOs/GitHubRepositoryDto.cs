namespace DevPulse.Application.DTOs;

public class GitHubRepositoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string? OwnerAvatarUrl { get; set; }
    public string? Description { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
    public int Stars { get; set; }
    public int Forks { get; set; }
    public int OpenIssues { get; set; }
    public int Watchers { get; set; }
    public string? PrimaryLanguage { get; set; }
    public string? DefaultBranch { get; set; }
    public string? License { get; set; }
    public bool IsArchived { get; set; }
    public bool IsFork { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PushedAt { get; set; }
}
