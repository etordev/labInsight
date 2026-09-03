using LabInsight.Api.Entities;

namespace LabInsight.Api.Repositories;

public interface IMetricDefinitionRepository : IRepository<MetricDefinitionEntity>
{
    Task<IReadOnlyList<MetricDefinitionEntity>> ListOrderedByTechnicalNameAsync(
        bool isDeleted,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListTechnicalNamesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, MetricDefinitionEntity>> GetByTechnicalNameAsync(
        CancellationToken cancellationToken);
}
