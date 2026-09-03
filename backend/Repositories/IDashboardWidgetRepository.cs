using LabInsight.Api.Entities;

namespace LabInsight.Api.Repositories;

public interface IDashboardWidgetRepository : IRepository<DashboardWidgetEntity>
{
    Task<IReadOnlyList<DashboardWidgetEntity>> ListWithTypesAsync(
        bool isDeleted,
        CancellationToken cancellationToken);

    Task<DashboardWidgetEntity?> GetWithTypesAsync(
        int id,
        bool isDeleted,
        CancellationToken cancellationToken);

    Task<DashboardWidgetEntity?> GetTrackedAsync(int id, bool isDeleted, CancellationToken cancellationToken);

    Task<IReadOnlyList<DashboardWidgetEntity>> GetTrackedByIdsAsync(
        IReadOnlyCollection<int> ids,
        bool isDeleted,
        CancellationToken cancellationToken);

    Task<int> GetMaxOrderingAsync(CancellationToken cancellationToken);

    Task LoadTypesAsync(DashboardWidgetEntity entity, CancellationToken cancellationToken);
}
