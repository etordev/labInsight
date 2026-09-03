using LabInsight.Api.DTOs;
using LabInsight.Api.Repositories;

namespace LabInsight.Api.Services;

public class VisualizationTypeService(IVisualizationTypeRepository visualizationTypeRepository) : IVisualizationTypeService
{
    public async Task<IReadOnlyList<VisualizationTypeDto>> GetVisualizationTypesAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        var types = await visualizationTypeRepository.ListOrderedByTechnicalNameAsync(
            isDeleted,
            cancellationToken);

        return types
            .Select(type => new VisualizationTypeDto
            {
                Id = type.Id,
                TechnicalName = type.TechnicalName
            })
            .ToList();
    }
}
