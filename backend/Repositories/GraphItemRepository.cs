using LabInsight.Api.Data;
using LabInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Repositories;

public class GraphItemRepository(LabInsightDbContext dbContext)
    : Repository<GraphItemEntity>(dbContext), IGraphItemRepository
{
    public async Task<IReadOnlyList<GraphItemEntity>> ListWithTypesAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        return await Query(isDeleted)
            .Include(item => item.GraphType)
            .Include(item => item.GraphDataType)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<GraphItemEntity?> GetWithTypesAsync(
        int id,
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        return Query(isDeleted)
            .Include(item => item.GraphType)
            .Include(item => item.GraphDataType)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task<GraphItemEntity?> GetTrackedAsync(
        int id,
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        return Query(isDeleted, asNoTracking: false)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task LoadTypesAsync(GraphItemEntity entity, CancellationToken cancellationToken)
    {
        await DbContext.Entry(entity).Reference(item => item.GraphType).LoadAsync(cancellationToken);
        await DbContext.Entry(entity).Reference(item => item.GraphDataType).LoadAsync(cancellationToken);
    }
}
