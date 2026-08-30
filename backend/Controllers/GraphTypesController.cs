using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class GraphTypesController(IGraphTypeService graphTypeService) : ControllerBase
{
    [HttpGet("getGraphTypes")]
    public async Task<ActionResult<IReadOnlyList<GraphTypeDto>>> GetGraphTypes(CancellationToken cancellationToken)
    {
        return Ok(await graphTypeService.GetGraphTypesAsync(cancellationToken));
    }
}
