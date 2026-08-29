using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api/analyses")]
public class AnalysesController(ILabAnalysisService labAnalysisService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<LabAnalysisDto>>> Get(
        [FromQuery] LabAnalysisQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await labAnalysisService.GetAsync(query, cancellationToken));
    }
}
