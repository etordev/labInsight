using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface IGraphItemService
{
    Task<IReadOnlyList<GraphItemDto>> GetAllAsync(CancellationToken cancellationToken);
}
