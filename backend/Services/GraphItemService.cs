using LabInsight.Api.Data;
using LabInsight.Api.DTOs;
using LabInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Services;

public class GraphItemService(LabInsightDbContext dbContext) : IGraphItemService
{
    public async Task<IReadOnlyList<GraphItemDto>> GetGraphItemsAsync(CancellationToken cancellationToken)
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

    public async Task<UpsertGraphItemResult> UpsertGraphItemAsync(
        UpsertGraphItemRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return new UpsertGraphItemResult(null, "Name is required.", false, false);
        }

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        var content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim();

        var graphTypeExists = await dbContext.GraphTypes
            .AnyAsync(type => type.Id == request.GraphTypeId, cancellationToken);
        if (!graphTypeExists)
        {
            return new UpsertGraphItemResult(null, "Graph type was not found.", true, false);
        }

        var graphDataTypeExists = await dbContext.GraphDataTypes
            .AnyAsync(type => type.Id == request.GraphDataTypeId, cancellationToken);
        if (!graphDataTypeExists)
        {
            return new UpsertGraphItemResult(null, "Graph data type was not found.", true, false);
        }

        GraphItemEntity entity;
        var created = false;

        if (request.Id is > 0)
        {
            var existing = await dbContext.GraphItems
                .FirstOrDefaultAsync(item => item.Id == request.Id.Value, cancellationToken);

            if (existing is null)
            {
                return new UpsertGraphItemResult(null, "Graph item was not found.", true, false);
            }

            entity = existing;
            entity.Name = name;
            entity.Description = description;
            entity.Content = content;
            entity.GraphTypeId = request.GraphTypeId;
            entity.GraphDataTypeId = request.GraphDataTypeId;
        }
        else
        {
            entity = new GraphItemEntity
            {
                Name = name,
                Description = description,
                Content = content,
                GraphTypeId = request.GraphTypeId,
                GraphDataTypeId = request.GraphDataTypeId
            };

            dbContext.GraphItems.Add(entity);
            created = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(entity).Reference(item => item.GraphType).LoadAsync(cancellationToken);
        await dbContext.Entry(entity).Reference(item => item.GraphDataType).LoadAsync(cancellationToken);

        return new UpsertGraphItemResult(MapDto(entity), null, false, created);
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
