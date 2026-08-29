using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api/graph-types")]
public class GraphTypesController(IGraphTypeService graphTypeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GraphTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await graphTypeService.GetAllAsync(cancellationToken));
    }
}
