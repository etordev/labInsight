using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface IGraphItemService
{
    Task<IReadOnlyList<GraphItemDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<CreateGraphItemResult> CreateAsync(
        CreateGraphItemRequest request,
        CancellationToken cancellationToken);
}

public sealed record CreateGraphItemResult(GraphItemDto? Item, string? Error, bool NotFound);
