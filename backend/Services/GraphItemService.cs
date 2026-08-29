using LabInsight.Api.Data;
using LabInsight.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Services;

public class GraphItemService(LabInsightDbContext dbContext) : IGraphItemService
{
    public async Task<IReadOnlyList<GraphItemDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.GraphItems
            .AsNoTracking()
            .OrderBy(i => i.Id)
            .Select(i => new GraphItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Content = i.Content,
                GraphTypeId = i.GraphTypeId,
                GraphDataTypeId = i.GraphDataTypeId,
                GraphType = new GraphTypeDto
                {
                    Id = i.GraphType.Id,
                    TechnicalName = i.GraphType.TechnicalName
                },
                GraphDataType = new GraphDataTypeDto
                {
                    Id = i.GraphDataType.Id,
                    TechnicalName = i.GraphDataType.TechnicalName
                }
            })
            .ToListAsync(cancellationToken);
    }
}
