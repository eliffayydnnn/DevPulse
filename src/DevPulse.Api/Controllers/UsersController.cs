using DevPulse.Application.Common;
using DevPulse.Application.Contracts;
using DevPulse.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IGitHubService _gitHubService;

    public UsersController(IGitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    [HttpGet("{username}")]
    [ProducesResponseType(typeof(ApiResponse<GitHubUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserProfile(string username, CancellationToken cancellationToken)
    {
        var data = await _gitHubService.GetUserProfileAsync(username, cancellationToken);
        return Ok(ApiResponse<GitHubUserDto>.Ok(data));
    }

    [HttpGet("{username}/repos")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GitHubRepositoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRepositories(string username, [FromQuery] int limit = 30, CancellationToken cancellationToken = default)
    {
        var data = await _gitHubService.GetUserRepositoriesAsync(username, limit, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GitHubRepositoryDto>>.Ok(data));
    }
}
