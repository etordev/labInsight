using LabInsight.Api.Data;
using LabInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Repositories;

public class DashboardWidgetRepository(LabInsightDbContext dbContext)
    : Repository<DashboardWidgetEntity>(dbContext), IDashboardWidgetRepository
{
    public async Task<IReadOnlyList<DashboardWidgetEntity>> ListWithTypesAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        return await Query(isDeleted)
            .Include(item => item.VisualizationType)
            .Include(item => item.MetricDefinition)
            .OrderBy(item => item.Ordering)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<DashboardWidgetEntity?> GetWithTypesAsync(
        int id,
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        return Query(isDeleted)
            .Include(item => item.VisualizationType)
            .Include(item => item.MetricDefinition)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task<DashboardWidgetEntity?> GetTrackedAsync(
        int id,
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        return Query(isDeleted, asNoTracking: false)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<DashboardWidgetEntity>> GetTrackedByIdsAsync(
        IReadOnlyCollection<int> ids,
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        return await Query(isDeleted, asNoTracking: false)
            .Where(item => ids.Contains(item.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetMaxOrderingAsync(CancellationToken cancellationToken)
    {
        return await Set
            .Where(item => !item.IsDeleted)
            .MaxAsync(item => (int?)item.Ordering, cancellationToken) ?? 0;
    }

    public async Task LoadTypesAsync(DashboardWidgetEntity entity, CancellationToken cancellationToken)
    {
        await DbContext.Entry(entity).Reference(item => item.VisualizationType).LoadAsync(cancellationToken);
        await DbContext.Entry(entity).Reference(item => item.MetricDefinition).LoadAsync(cancellationToken);
    }
}
