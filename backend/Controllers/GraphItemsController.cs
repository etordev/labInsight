using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class GraphItemsController(IGraphItemService graphItemService, IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("getGraphItemData/{id:int}")]
    public async Task<ActionResult<GraphItemAnalyticsDto>> GetGraphItemData(
        int id,
        [FromQuery] bool isDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var analytics = await analyticsService.GetGraphItemDataAsync(id, isDeleted, cancellationToken);
        if (analytics is null)
        {
            return NotFound(new { message = "Graph item was not found." });
        }

        return Ok(analytics);
    }

    [HttpGet("getGraphItems")]
    public async Task<ActionResult<IReadOnlyList<GraphItemDto>>> GetGraphItems(
        [FromQuery] bool isDeleted = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(await graphItemService.GetGraphItemsAsync(isDeleted, cancellationToken));
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

    [HttpDelete("deleteGraphItem/{id:int}")]
    public async Task<IActionResult> DeleteGraphItem(int id, CancellationToken cancellationToken)
    {
        var deleted = await graphItemService.DeleteGraphItemAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = "Graph item was not found." });
        }

        return NoContent();
    }

    [HttpPost("updateGraphOrdering")]
    public async Task<IActionResult> UpdateGraphOrdering(
        [FromBody] IReadOnlyList<UpdateGraphOrderingItem> items,
        CancellationToken cancellationToken)
    {
        var error = await graphItemService.UpdateGraphOrderingAsync(items, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return NoContent();
    }
}
