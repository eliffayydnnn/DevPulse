using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHostEnvironment _env;

    public HealthController(IHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "DevPulse.Api",
            version = "1.0.0",
            environment = _env.EnvironmentName,
            timestampUtc = DateTime.UtcNow
        });
    }
}
