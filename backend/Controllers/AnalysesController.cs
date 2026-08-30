using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class AnalysesController(ILabAnalysisService labAnalysisService) : ControllerBase
{
    [HttpGet("getAnalyses")]
    public async Task<ActionResult<PagedResultDto<LabAnalysisDto>>> GetAnalyses(
        [FromQuery] LabAnalysisQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await labAnalysisService.GetAnalysesAsync(query, cancellationToken));
    }
}
