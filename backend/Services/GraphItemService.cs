using LabInsight.Api.Data;
using LabInsight.Api.DTOs;
using LabInsight.Api.Entities;
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

    public async Task<CreateGraphItemResult> CreateAsync(
        CreateGraphItemRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return new CreateGraphItemResult(null, "Name is required.", false);
        }

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        var graphTypeExists = await dbContext.GraphTypes
            .AnyAsync(type => type.Id == request.GraphTypeId, cancellationToken);
        if (!graphTypeExists)
        {
            return new CreateGraphItemResult(null, "Graph type was not found.", true);
        }

        var graphDataTypeExists = await dbContext.GraphDataTypes
            .AnyAsync(type => type.Id == request.GraphDataTypeId, cancellationToken);
        if (!graphDataTypeExists)
        {
            return new CreateGraphItemResult(null, "Graph data type was not found.", true);
        }

        var entity = new GraphItemEntity
        {
            Name = name,
            Description = description,
            Content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim(),
            GraphTypeId = request.GraphTypeId,
            GraphDataTypeId = request.GraphDataTypeId
        };

        dbContext.GraphItems.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(entity).Reference(item => item.GraphType).LoadAsync(cancellationToken);
        await dbContext.Entry(entity).Reference(item => item.GraphDataType).LoadAsync(cancellationToken);

        return new CreateGraphItemResult(MapDto(entity), null, false);
    }

    private static GraphItemDto MapDto(GraphItemEntity item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        Content = item.Content,
        GraphTypeId = item.GraphTypeId,
        GraphDataTypeId = item.GraphDataTypeId,
        GraphType = new GraphTypeDto
        {
            Id = item.GraphType.Id,
            TechnicalName = item.GraphType.TechnicalName
        },
        GraphDataType = new GraphDataTypeDto
        {
            Id = item.GraphDataType.Id,
            TechnicalName = item.GraphDataType.TechnicalName
        }
    };
}
