using LabInsight.Api.Data;
using LabInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Repositories;

public class GraphTypeRepository(LabInsightDbContext dbContext)
    : Repository<GraphTypeEntity>(dbContext), IGraphTypeRepository
{
    public async Task<IReadOnlyList<GraphTypeEntity>> ListOrderedByTechnicalNameAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        return await Query(isDeleted)
            .OrderBy(type => type.TechnicalName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ListTechnicalNamesAsync(CancellationToken cancellationToken)
    {
        return await Set
            .Select(type => type.TechnicalName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, GraphTypeEntity>> GetByTechnicalNameAsync(
        CancellationToken cancellationToken)
    {
        return await Set.ToDictionaryAsync(type => type.TechnicalName, cancellationToken);
    }
}
