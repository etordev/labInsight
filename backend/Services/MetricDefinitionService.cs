using LabInsight.Api.DTOs;
using LabInsight.Api.Repositories;

namespace LabInsight.Api.Services;

public class MetricDefinitionService(IMetricDefinitionRepository metricDefinitionRepository) : IMetricDefinitionService
{
    public async Task<IReadOnlyList<MetricDefinitionDto>> GetMetricDefinitionsAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        var types = await metricDefinitionRepository.ListOrderedByTechnicalNameAsync(
            isDeleted,
            cancellationToken);

        return types
            .Select(type => new MetricDefinitionDto
            {
                Id = type.Id,
                TechnicalName = type.TechnicalName
            })
            .ToList();
    }
}
