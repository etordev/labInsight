using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class DashboardWidgetsController(IDashboardWidgetService dashboardWidgetService, IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("getDashboardWidgetData/{id:int}")]
    public async Task<ActionResult<DashboardWidgetAnalyticsDto>> GetDashboardWidgetData(
        int id,
        [FromQuery] bool isDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var analytics = await analyticsService.GetDashboardWidgetDataAsync(id, isDeleted, cancellationToken);
        if (analytics is null)
        {
            return NotFound(new { message = "Dashboard widget was not found." });
        }

        return Ok(analytics);
    }

    [HttpGet("getDashboardWidgets")]
    public async Task<ActionResult<IReadOnlyList<DashboardWidgetDto>>> GetDashboardWidgets(
        [FromQuery] bool isDeleted = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(await dashboardWidgetService.GetDashboardWidgetsAsync(isDeleted, cancellationToken));
    }

    [HttpPost("upsertDashboardWidget")]
    public async Task<ActionResult<DashboardWidgetDto>> UpsertDashboardWidget(
        [FromBody] UpsertDashboardWidgetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await dashboardWidgetService.UpsertDashboardWidgetAsync(request, cancellationToken);

        if (result.Item is not null)
        {
            return result.Created
                ? Created($"/api/getDashboardWidgets", result.Item)
                : Ok(result.Item);
        }

        if (result.NotFound)
        {
            return NotFound(new { message = result.Error });
        }

        return BadRequest(new { message = result.Error });
    }

    [HttpDelete("deleteDashboardWidget/{id:int}")]
    public async Task<IActionResult> DeleteDashboardWidget(int id, CancellationToken cancellationToken)
    {
        var deleted = await dashboardWidgetService.DeleteDashboardWidgetAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = "Dashboard widget was not found." });
        }

        return NoContent();
    }

    [HttpPost("updateDashboardWidgetOrdering")]
    public async Task<IActionResult> UpdateDashboardWidgetOrdering(
        [FromBody] IReadOnlyList<UpdateDashboardWidgetOrderingItem> items,
        CancellationToken cancellationToken)
    {
        var error = await dashboardWidgetService.UpdateDashboardWidgetOrderingAsync(items, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return NoContent();
    }
}
