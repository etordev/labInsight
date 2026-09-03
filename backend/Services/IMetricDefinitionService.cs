using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface IMetricDefinitionService
{
    Task<IReadOnlyList<MetricDefinitionDto>> GetMetricDefinitionsAsync(
        bool isDeleted,
        CancellationToken cancellationToken);
}
