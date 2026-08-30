using LabInsight.Api.Entities;

namespace LabInsight.Api.Repositories;

public interface IGraphTypeRepository : IRepository<GraphTypeEntity>
{
    Task<IReadOnlyList<GraphTypeEntity>> ListOrderedByTechnicalNameAsync(
        bool isDeleted,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListTechnicalNamesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, GraphTypeEntity>> GetByTechnicalNameAsync(
        CancellationToken cancellationToken);
}
