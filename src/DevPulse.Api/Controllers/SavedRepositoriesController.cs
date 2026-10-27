using System.Security.Claims;
using DevPulse.Application.Common;
using DevPulse.Application.Contracts;
using DevPulse.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

public class SaveRepositoryRequest
{
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

[Authorize]
[ApiController]
[Route("api/saved-repositories")]
public class SavedRepositoriesController : ControllerBase
{
    private readonly ISavedRepositoryService _savedRepositoryService;

    public SavedRepositoriesController(ISavedRepositoryService savedRepositoryService)
    {
        _savedRepositoryService = savedRepositoryService;
    }

    private Guid GetCurrentUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idStr, out var id))
        {
            return id;
        }
        throw new UnauthorizedAccessException("Valid user identity claim was not found in token.");
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SavedRepository>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSaved(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var list = await _savedRepositoryService.GetSavedRepositoriesAsync(userId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SavedRepository>>.Ok(list));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SavedRepository>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Save([FromBody] SaveRepositoryRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var saved = await _savedRepositoryService.SaveRepositoryAsync(userId, request.Owner, request.Name, request.Notes, cancellationToken);
        return Ok(ApiResponse<SavedRepository>.Ok(saved));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _savedRepositoryService.DeleteSavedRepositoryAsync(userId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
