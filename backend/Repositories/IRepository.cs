using LabInsight.Api.Entities;

namespace LabInsight.Api.Repositories;

public interface IRepository<TEntity>
    where TEntity : EntityBase
{
    IQueryable<TEntity> Query(bool isDeleted, bool asNoTracking = true);

    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken, bool asNoTracking = false);

    Task<bool> ExistsAsync(int id, bool isDeleted, CancellationToken cancellationToken);

    Task<bool> AnyAsync(CancellationToken cancellationToken);

    Task<int> CountAsync(CancellationToken cancellationToken);

    void Add(TEntity entity);

    void AddRange(IEnumerable<TEntity> entities);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);

    Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    void ClearTracked();
}
