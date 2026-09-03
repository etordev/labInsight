using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface IVisualizationTypeService
{
    Task<IReadOnlyList<VisualizationTypeDto>> GetVisualizationTypesAsync(bool isDeleted, CancellationToken cancellationToken);
}
