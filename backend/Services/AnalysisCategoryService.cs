using LabInsight.Api.Data;
using LabInsight.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Services;

public class AnalysisCategoryService(LabInsightDbContext dbContext) : IAnalysisCategoryService
{
    public async Task<IReadOnlyList<AnalysisCategoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.AnalysisCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new AnalysisCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                ExpectedProcessingHours = c.ExpectedProcessingHours
            })
            .ToListAsync(cancellationToken);
    }
}
