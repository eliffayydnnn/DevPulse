using DevPulse.Application.Common;
using DevPulse.Application.Contracts;
using DevPulse.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

public class SaveRepositoryRequest
{
    public string Owner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class SavedRepositoriesController : ControllerBase
{
    private readonly ISavedRepositoryService _savedRepositoryService;
    // Default system user id for demo/portfolio mode until full auth login is executed
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public SavedRepositoriesController(ISavedRepositoryService savedRepositoryService)
    {
        _savedRepositoryService = savedRepositoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SavedRepository>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSaved(CancellationToken cancellationToken)
    {
        var list = await _savedRepositoryService.GetSavedRepositoriesAsync(DemoUserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SavedRepository>>.Ok(list));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SavedRepository>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Save([FromBody] SaveRepositoryRequest request, CancellationToken cancellationToken)
    {
        var saved = await _savedRepositoryService.SaveRepositoryAsync(DemoUserId, request.Owner, request.Name, request.Notes, cancellationToken);
        return Ok(ApiResponse<SavedRepository>.Ok(saved));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _savedRepositoryService.DeleteSavedRepositoryAsync(DemoUserId, id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true));
    }
}
