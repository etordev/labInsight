using LabInsight.Api.Data;
using LabInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Repositories;

public class Repository<TEntity>(LabInsightDbContext dbContext) : IRepository<TEntity>
    where TEntity : EntityBase
{
    protected LabInsightDbContext DbContext { get; } = dbContext;

    protected DbSet<TEntity> Set => DbContext.Set<TEntity>();

    public IQueryable<TEntity> Query(bool isDeleted, bool asNoTracking = true)
    {
        IQueryable<TEntity> query = Set.Where(entity => entity.IsDeleted == isDeleted);
        return asNoTracking ? query.AsNoTracking() : query;
    }

    public Task<TEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        bool asNoTracking = false)
    {
        var query = asNoTracking ? Set.AsNoTracking() : Set.AsQueryable();
        return query.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(int id, bool isDeleted, CancellationToken cancellationToken)
    {
        return Set.AnyAsync(entity => entity.Id == id && entity.IsDeleted == isDeleted, cancellationToken);
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken)
    {
        return Set.AnyAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return Set.CountAsync(cancellationToken);
    }

    public void Add(TEntity entity)
    {
        Set.Add(entity);
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        Set.AddRange(entities);
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        return Set.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken)
    {
        var existing = await Set.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        if (existing.IsDeleted)
        {
            return true;
        }

        existing.IsDeleted = true;
        await DbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return DbContext.SaveChangesAsync(cancellationToken);
    }

    public void ClearTracked()
    {
        DbContext.ChangeTracker.Clear();
    }
}
