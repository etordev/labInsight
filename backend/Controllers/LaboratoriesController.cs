using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api/laboratories")]
public class LaboratoriesController(ILaboratoryService laboratoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LaboratoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await laboratoryService.GetAllAsync(cancellationToken));
    }
}
