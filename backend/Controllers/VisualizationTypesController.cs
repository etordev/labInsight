using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class VisualizationTypesController(IVisualizationTypeService visualizationTypeService) : ControllerBase
{
    [HttpGet("getVisualizationTypes")]
    public async Task<ActionResult<IReadOnlyList<VisualizationTypeDto>>> GetVisualizationTypes(
        [FromQuery] bool isDeleted = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(await visualizationTypeService.GetVisualizationTypesAsync(isDeleted, cancellationToken));
    }
}
