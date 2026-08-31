using LabInsight.Api.DTOs;
using LabInsight.Api.Entities;
using LabInsight.Api.Repositories;

namespace LabInsight.Api.Services;

public class GraphItemService(
    IGraphItemRepository graphItemRepository,
    IGraphTypeRepository graphTypeRepository,
    IGraphDataTypeRepository graphDataTypeRepository) : IGraphItemService
{
    public async Task<IReadOnlyList<GraphItemDto>> GetGraphItemsAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        var items = await graphItemRepository.ListWithTypesAsync(isDeleted, cancellationToken);
        return items.Select(MapDto).ToList();
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

        var graphTypeExists = await graphTypeRepository.ExistsAsync(
            request.GraphTypeId,
            isDeleted: false,
            cancellationToken);
        if (!graphTypeExists)
        {
            return new UpsertGraphItemResult(null, "Graph type was not found.", true, false);
        }

        var graphDataTypeExists = await graphDataTypeRepository.ExistsAsync(
            request.GraphDataTypeId,
            isDeleted: false,
            cancellationToken);
        if (!graphDataTypeExists)
        {
            return new UpsertGraphItemResult(null, "Graph data type was not found.", true, false);
        }

        GraphItemEntity entity;
        var created = false;

        if (request.Id is > 0)
        {
            var existing = await graphItemRepository.GetTrackedAsync(
                request.Id.Value,
                isDeleted: false,
                cancellationToken);

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
                GraphDataTypeId = request.GraphDataTypeId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                Ordering = await graphItemRepository.GetMaxOrderingAsync(cancellationToken) + 1
            };

            graphItemRepository.Add(entity);
            created = true;
        }

        await graphItemRepository.SaveChangesAsync(cancellationToken);
        await graphItemRepository.LoadTypesAsync(entity, cancellationToken);

        return new UpsertGraphItemResult(MapDto(entity), null, false, created);
    }

    public Task<bool> DeleteGraphItemAsync(int id, CancellationToken cancellationToken)
    {
        return graphItemRepository.SoftDeleteAsync(id, cancellationToken);
    }

    public async Task<string?> UpdateGraphOrderingAsync(
        IReadOnlyList<UpdateGraphOrderingItem> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return "At least one graph is required.";
        }

        var graphIds = items.Select(item => item.GraphId).ToList();
        if (graphIds.Distinct().Count() != graphIds.Count)
        {
            return "Each graph can appear only once.";
        }

        var orderings = items.Select(item => item.Ordering).ToList();
        if (orderings.Any(ordering => ordering < 1) || orderings.Distinct().Count() != orderings.Count)
        {
            return "Ordering values must be unique positive numbers.";
        }

        var existing = await graphItemRepository.GetTrackedByIdsAsync(graphIds, isDeleted: false, cancellationToken);
        if (existing.Count != graphIds.Count)
        {
            return "One or more graphs were not found.";
        }

        var orderingById = items.ToDictionary(item => item.GraphId, item => item.Ordering);
        foreach (var entity in existing)
        {
            entity.Ordering = orderingById[entity.Id];
        }

        await graphItemRepository.SaveChangesAsync(cancellationToken);
        return null;
    }

    private static GraphItemDto MapDto(GraphItemEntity item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        Content = item.Content,
        GraphTypeId = item.GraphTypeId,
        GraphDataTypeId = item.GraphDataTypeId,
        Ordering = item.Ordering,
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
