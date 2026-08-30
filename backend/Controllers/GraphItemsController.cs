using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class GraphItemsController(IGraphItemService graphItemService) : ControllerBase
{
    [HttpGet("getGraphItems")]
    public async Task<ActionResult<IReadOnlyList<GraphItemDto>>> GetGraphItems(CancellationToken cancellationToken)
    {
        return Ok(await graphItemService.GetGraphItemsAsync(cancellationToken));
    }

    [HttpPost("upsertGraphItem")]
    public async Task<ActionResult<GraphItemDto>> UpsertGraphItem(
        [FromBody] UpsertGraphItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await graphItemService.UpsertGraphItemAsync(request, cancellationToken);

        if (result.Item is not null)
        {
            return result.Created
                ? Created($"/api/getGraphItems", result.Item)
                : Ok(result.Item);
        }

        if (result.NotFound)
        {
            return NotFound(new { message = result.Error });
        }

        return BadRequest(new { message = result.Error });
    }
}
