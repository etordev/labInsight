using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class MetricDefinitionsController(IMetricDefinitionService metricDefinitionService) : ControllerBase
{
    [HttpGet("getMetricDefinitions")]
    public async Task<ActionResult<IReadOnlyList<MetricDefinitionDto>>> GetMetricDefinitions(
        [FromQuery] bool isDeleted = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(await metricDefinitionService.GetMetricDefinitionsAsync(isDeleted, cancellationToken));
    }
}
