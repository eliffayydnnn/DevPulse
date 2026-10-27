using System.Text.RegularExpressions;
using DevPulse.Domain.Exceptions;

namespace DevPulse.Application.Common;

public static partial class GitHubQueryParser
{
    private static readonly Regex GitHubRepoRegex = new(
        @"^(?:https?:\/\/)?(?:www\.)?github\.com\/([a-zA-Z0-9_.-]+)\/([a-zA-Z0-9_.-]+?)(?:\.git|\/.*)?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex OwnerRepoRegex = new(
        @"^([a-zA-Z0-9_.-]+)\/([a-zA-Z0-9_.-]+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static (string Owner, string Repo) Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(input), new[] { "Repository target cannot be empty." } }
            });
        }

        var trimmed = input.Trim().TrimEnd('/');

        // Case 1: Full URL or URL-like format (e.g. https://github.com/owner/repo)
        var urlMatch = GitHubRepoRegex.Match(trimmed);
        if (urlMatch.Success)
        {
            var owner = urlMatch.Groups[1].Value;
            var repo = urlMatch.Groups[2].Value;
            return CleanAndValidate(owner, repo);
        }

        // Case 2: Short form (e.g. owner/repo)
        var shortMatch = OwnerRepoRegex.Match(trimmed);
        if (shortMatch.Success)
        {
            var owner = shortMatch.Groups[1].Value;
            var repo = shortMatch.Groups[2].Value;
            return CleanAndValidate(owner, repo);
        }

        throw new ValidationException(new Dictionary<string, string[]>
        {
            { nameof(input), new[] { $"Invalid GitHub repository format: '{input}'. Expected 'owner/repo' or 'https://github.com/owner/repo'." } }
        });
    }

    private static (string Owner, string Repo) CleanAndValidate(string owner, string repo)
    {
        // Clean common suffixes like .git
        if (repo.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            repo = repo[..^4];
        }

        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
        {
            throw new ValidationException("Owner and repository names cannot be empty.");
        }

        return (owner, repo);
    }
}
