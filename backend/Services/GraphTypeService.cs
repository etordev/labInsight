using LabInsight.Api.Data;
using LabInsight.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Services;

public class GraphTypeService(LabInsightDbContext dbContext) : IGraphTypeService
{
    public async Task<IReadOnlyList<GraphTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.GraphTypes
            .AsNoTracking()
            .OrderBy(t => t.TechnicalName)
            .Select(t => new GraphTypeDto
            {
                Id = t.Id,
                TechnicalName = t.TechnicalName
            })
            .ToListAsync(cancellationToken);
    }
}
