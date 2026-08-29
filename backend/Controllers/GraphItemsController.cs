using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api/graph-items")]
public class GraphItemsController(IGraphItemService graphItemService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GraphItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await graphItemService.GetAllAsync(cancellationToken));
    }
}
