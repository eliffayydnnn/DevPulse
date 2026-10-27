namespace DevPulse.Application.DTOs;

public class GitHubUserDto
{
    public string Login { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? Company { get; set; }
    public string? Location { get; set; }
    public string? Blog { get; set; }
    public string? TwitterUsername { get; set; }
    public int PublicRepos { get; set; }
    public int Followers { get; set; }
    public int Following { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
