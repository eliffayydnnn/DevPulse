using DevPulse.Application.Common;
using DevPulse.Application.Contracts;
using DevPulse.Application.DTOs;
using DevPulse.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepositoriesController : ControllerBase
{
    private readonly IGitHubService _gitHubService;

    public RepositoriesController(IGitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    [HttpPost("parse")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ParseQuery([FromBody] SearchRepositoryRequest request)
    {
        var (owner, repo) = GitHubQueryParser.Parse(request.Query);
        return Ok(ApiResponse<object>.Ok(new { owner, repo, query = request.Query }));
    }

    [HttpGet("{owner}/{repo}")]
    [ProducesResponseType(typeof(ApiResponse<GitHubRepositoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRepository(string owner, string repo, CancellationToken cancellationToken)
    {
        var data = await _gitHubService.GetRepositoryAsync(owner, repo, cancellationToken);
        return Ok(ApiResponse<GitHubRepositoryDto>.Ok(data));
    }

    [HttpGet("{owner}/{repo}/commits")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CommitSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommits(string owner, string repo, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        var data = await _gitHubService.GetCommitsAsync(owner, repo, limit, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CommitSummaryDto>>.Ok(data));
    }

    [HttpGet("{owner}/{repo}/pulls")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PullRequestSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPullRequests(string owner, string repo, [FromQuery] ItemStateFilter state = ItemStateFilter.All, [FromQuery] int limit = 30, CancellationToken cancellationToken = default)
    {
        var data = await _gitHubService.GetPullRequestsAsync(owner, repo, state, limit, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PullRequestSummaryDto>>.Ok(data));
    }

    [HttpGet("{owner}/{repo}/issues")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<IssueSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIssues(string owner, string repo, [FromQuery] ItemStateFilter state = ItemStateFilter.All, [FromQuery] int limit = 30, CancellationToken cancellationToken = default)
    {
        var data = await _gitHubService.GetIssuesAsync(owner, repo, state, limit, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<IssueSummaryDto>>.Ok(data));
    }

    [HttpGet("{owner}/{repo}/languages")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LanguageDistributionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLanguages(string owner, string repo, CancellationToken cancellationToken)
    {
        var data = await _gitHubService.GetLanguagesAsync(owner, repo, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<LanguageDistributionDto>>.Ok(data));
    }
}
