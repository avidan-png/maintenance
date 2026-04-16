using Maintenance.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController(SettingsService service) : ControllerBase
{
    [HttpGet("preavis-default")]
    public async Task<IActionResult> GetPreavisDefault()
    {
        var mois = await service.GetPreavisDefaultAsync();
        return Ok(new { mois });
    }

    [HttpPatch("preavis-default")]
    public async Task<IActionResult> SetPreavisDefault([FromBody] PreavisDefaultRequest request)
    {
        if (request.Mois < 1 || request.Mois > 12)
            return BadRequest("Le délai doit être entre 1 et 12 mois");

        await service.SetPreavisDefaultAsync(request.Mois);
        return Ok(new { mois = request.Mois });
    }
}

public record PreavisDefaultRequest(int Mois);
