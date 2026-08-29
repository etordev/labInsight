using LabInsight.Api.DTOs;
using LabInsight.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LabInsight.Api.Controllers;

[ApiController]
[Route("api/analysis-categories")]
public class AnalysisCategoriesController(IAnalysisCategoryService analysisCategoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AnalysisCategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await analysisCategoryService.GetAllAsync(cancellationToken));
    }
}
