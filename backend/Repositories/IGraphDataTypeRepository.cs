using LabInsight.Api.Entities;

namespace LabInsight.Api.Repositories;

public interface IGraphDataTypeRepository : IRepository<GraphDataTypeEntity>
{
    Task<IReadOnlyList<GraphDataTypeEntity>> ListOrderedByTechnicalNameAsync(
        bool isDeleted,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListTechnicalNamesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, GraphDataTypeEntity>> GetByTechnicalNameAsync(
        CancellationToken cancellationToken);
}
