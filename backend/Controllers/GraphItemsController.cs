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

    [HttpPost]
    public async Task<ActionResult<GraphItemDto>> Create(
        [FromBody] CreateGraphItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await graphItemService.CreateAsync(request, cancellationToken);

        if (result.Item is not null)
        {
            return Created($"/api/graph-items/{result.Item.Id}", result.Item);
        }

        if (result.NotFound)
        {
            return NotFound(new { message = result.Error });
        }

        return BadRequest(new { message = result.Error });
    }
}
