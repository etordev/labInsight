using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class LaboratoriesController(ILaboratoryService laboratoryService) : ControllerBase
{
    [HttpGet("getLaboratories")]
    public async Task<ActionResult<IReadOnlyList<LaboratoryDto>>> GetLaboratories(
        CancellationToken cancellationToken)
    {
        return Ok(await laboratoryService.GetLaboratoriesAsync(cancellationToken));
    }
}
