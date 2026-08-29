using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api/graph-data-types")]
public class GraphDataTypesController(IGraphDataTypeService graphDataTypeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GraphDataTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await graphDataTypeService.GetAllAsync(cancellationToken));
    }
}
