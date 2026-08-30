using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface IGraphTypeService
{
    Task<IReadOnlyList<GraphTypeDto>> GetGraphTypesAsync(CancellationToken cancellationToken);
}
