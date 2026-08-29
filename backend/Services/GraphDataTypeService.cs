using LabInsight.Api.Data;
using LabInsight.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Services;

public class GraphDataTypeService(LabInsightDbContext dbContext) : IGraphDataTypeService
{
    public async Task<IReadOnlyList<GraphDataTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.GraphDataTypes
            .AsNoTracking()
            .OrderBy(t => t.TechnicalName)
            .Select(t => new GraphDataTypeDto
            {
                Id = t.Id,
                TechnicalName = t.TechnicalName
            })
            .ToListAsync(cancellationToken);
    }
}
