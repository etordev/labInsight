using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface IGraphDataTypeService
{
    Task<IReadOnlyList<GraphDataTypeDto>> GetGraphDataTypesAsync(CancellationToken cancellationToken);
}
