using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface IGraphItemService
{
    Task<IReadOnlyList<GraphItemDto>> GetGraphItemsAsync(CancellationToken cancellationToken);

    Task<UpsertGraphItemResult> UpsertGraphItemAsync(
        UpsertGraphItemRequest request,
        CancellationToken cancellationToken);
}

public sealed record UpsertGraphItemResult(GraphItemDto? Item, string? Error, bool NotFound, bool Created);
