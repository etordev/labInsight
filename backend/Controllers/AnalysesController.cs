using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class AnalysesController(ILabAnalysisService labAnalysisService) : ControllerBase
{
    [HttpGet("getLabAnalyses")]
    public async Task<ActionResult<PagedResultDto<LabAnalysisDto>>> GetLabAnalyses(
        [FromQuery] LabAnalysisQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await labAnalysisService.GetLabAnalysesAsync(query, cancellationToken));
    }
}
