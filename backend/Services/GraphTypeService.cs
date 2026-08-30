using LabInsight.Api.DTOs;
using LabInsight.Api.Repositories;

namespace LabInsight.Api.Services;

public class GraphTypeService(IGraphTypeRepository graphTypeRepository) : IGraphTypeService
{
    public async Task<IReadOnlyList<GraphTypeDto>> GetGraphTypesAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        var types = await graphTypeRepository.ListOrderedByTechnicalNameAsync(
            isDeleted,
            cancellationToken);

        return types
            .Select(type => new GraphTypeDto
            {
                Id = type.Id,
                TechnicalName = type.TechnicalName
            })
            .ToList();
    }
}
