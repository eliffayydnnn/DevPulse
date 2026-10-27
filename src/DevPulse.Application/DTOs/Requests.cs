namespace DevPulse.Application.DTOs;

public class SearchRepositoryRequest
{
    /// <summary>
    /// Can be owner/repo (e.g. 'dotnet/aspnetcore') or a full GitHub URL (e.g. 'https://github.com/dotnet/aspnetcore')
    /// </summary>
    public string Query { get; set; } = string.Empty;
}

public class CompareRepositoriesRequest
{
    /// <summary>
    /// List of repository queries (minimum 2, maximum 4)
    /// </summary>
    public List<string> Repositories { get; set; } = new();
}
