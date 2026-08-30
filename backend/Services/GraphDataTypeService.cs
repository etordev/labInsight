using LabInsight.Api.DTOs;
using LabInsight.Api.Repositories;

namespace LabInsight.Api.Services;

public class GraphDataTypeService(IGraphDataTypeRepository graphDataTypeRepository) : IGraphDataTypeService
{
    public async Task<IReadOnlyList<GraphDataTypeDto>> GetGraphDataTypesAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        var types = await graphDataTypeRepository.ListOrderedByTechnicalNameAsync(
            isDeleted,
            cancellationToken);

        return types
            .Select(type => new GraphDataTypeDto
            {
                Id = type.Id,
                TechnicalName = type.TechnicalName
            })
            .ToList();
    }
}
