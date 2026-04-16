using Maintenance.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Maintenance.API.Controllers;

[ApiController]
[Route("api/contracts")]
public class ContractsController(ContractService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpPost("import/excel")]
    public async Task<IActionResult> ImportExcel(
        IFormFile file,
        [FromForm] int? delaiPreavisMois = null)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Fichier manquant");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var result = await service.ImportFromExcelAsync(ms.ToArray(), delaiPreavisMois);
        return Ok(result);
    }
}
