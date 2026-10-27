using DevPulse.Application.Common;
using DevPulse.Application.Contracts;
using DevPulse.Application.DTOs;
using DevPulse.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IGitHubService _gitHubService;
    private readonly IValidator<CompareRepositoriesRequest> _compareValidator;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        IGitHubService gitHubService,
        IValidator<CompareRepositoriesRequest> compareValidator)
    {
        _analyticsService = analyticsService;
        _gitHubService = gitHubService;
        _compareValidator = compareValidator;
    }

    [HttpGet("repository/{owner}/{repo}")]
    [ProducesResponseType(typeof(ApiResponse<RepositoryAnalysisDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnalyzeRepository(string owner, string repo, CancellationToken cancellationToken)
    {
        var analysis = await _analyticsService.AnalyzeRepositoryAsync(owner, repo, cancellationToken);
        return Ok(ApiResponse<RepositoryAnalysisDto>.Ok(analysis));
    }

    [HttpPost("compare")]
    [ProducesResponseType(typeof(ApiResponse<RepositoryComparisonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompareRepositories([FromBody] CompareRepositoriesRequest request, CancellationToken cancellationToken)
    {
        var validation = await _compareValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new DevPulse.Domain.Exceptions.ValidationException(errors);
        }

        var parsed = new List<(string Owner, string Repo)>();
        foreach (var item in request.Repositories)
        {
            parsed.Add(GitHubQueryParser.Parse(item));
        }

        var comparison = await _analyticsService.CompareRepositoriesAsync(parsed, cancellationToken);
        return Ok(ApiResponse<RepositoryComparisonDto>.Ok(comparison));
    }

    [HttpGet("ratelimit")]
    [ProducesResponseType(typeof(ApiResponse<RateLimitInfoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRateLimit(CancellationToken cancellationToken)
    {
        var rateLimit = await _gitHubService.GetRateLimitAsync(cancellationToken);
        return Ok(ApiResponse<RateLimitInfoDto>.Ok(rateLimit));
    }
}
