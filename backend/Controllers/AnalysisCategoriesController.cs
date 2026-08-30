using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api")]
public class AnalysisCategoriesController(IAnalysisCategoryService analysisCategoryService) : ControllerBase
{
    [HttpGet("getAnalysisCategories")]
    public async Task<ActionResult<IReadOnlyList<AnalysisCategoryDto>>> GetAnalysisCategories(
        [FromQuery] bool isDeleted = false,
        CancellationToken cancellationToken = default)
    {
        return Ok(await analysisCategoryService.GetAnalysisCategoriesAsync(isDeleted, cancellationToken));
    }
}
