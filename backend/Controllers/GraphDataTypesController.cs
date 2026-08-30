using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class GraphDataTypesController(IGraphDataTypeService graphDataTypeService) : ControllerBase
{
    [HttpGet("getGraphDataTypes")]
    public async Task<ActionResult<IReadOnlyList<GraphDataTypeDto>>> GetGraphDataTypes(
        CancellationToken cancellationToken)
    {
        return Ok(await graphDataTypeService.GetGraphDataTypesAsync(cancellationToken));
    }
}
