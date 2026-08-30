using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class GraphTypesController(IGraphTypeService graphTypeService) : ControllerBase
{
    [HttpGet("getGraphTypes")]
    public async Task<ActionResult<IReadOnlyList<GraphTypeDto>>> GetGraphTypes(
        [FromQuery] bool isDeleted = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(await graphTypeService.GetGraphTypesAsync(isDeleted, cancellationToken));
    }
}
