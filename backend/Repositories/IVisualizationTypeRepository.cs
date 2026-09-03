using LabInsight.Api.Entities;

namespace LabInsight.Api.Repositories;

public interface IVisualizationTypeRepository : IRepository<VisualizationTypeEntity>
{
    Task<IReadOnlyList<VisualizationTypeEntity>> ListOrderedByTechnicalNameAsync(
        bool isDeleted,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListTechnicalNamesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, VisualizationTypeEntity>> GetByTechnicalNameAsync(
        CancellationToken cancellationToken);
}
