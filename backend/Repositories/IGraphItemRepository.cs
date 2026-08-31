using LabInsight.Api.Entities;

namespace LabInsight.Api.Repositories;

public interface IGraphItemRepository : IRepository<GraphItemEntity>
{
    Task<IReadOnlyList<GraphItemEntity>> ListWithTypesAsync(
        bool isDeleted,
        CancellationToken cancellationToken);

    Task<GraphItemEntity?> GetWithTypesAsync(
        int id,
        bool isDeleted,
        CancellationToken cancellationToken);

    Task<GraphItemEntity?> GetTrackedAsync(int id, bool isDeleted, CancellationToken cancellationToken);

    Task<IReadOnlyList<GraphItemEntity>> GetTrackedByIdsAsync(
        IReadOnlyCollection<int> ids,
        bool isDeleted,
        CancellationToken cancellationToken);

    Task<int> GetMaxOrderingAsync(CancellationToken cancellationToken);

    Task LoadTypesAsync(GraphItemEntity entity, CancellationToken cancellationToken);
}
