// backend/Maintenance.API/Controllers/AlertsController.cs
using Maintenance.API.DTOs;
using Maintenance.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController(AlertsService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await service.GetAlertSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await service.GetAlertSettingsAsync();
        return Ok(settings);
    }

    [HttpPatch("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] AlertSettingsDto dto)
    {
        await service.UpdateAlertSettingsAsync(dto);
        return Ok(dto);
    }
}
