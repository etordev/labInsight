using LabInsight.Api.Data;
using LabInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Repositories;

public class GraphDataTypeRepository(LabInsightDbContext dbContext)
    : Repository<GraphDataTypeEntity>(dbContext), IGraphDataTypeRepository
{
    public async Task<IReadOnlyList<GraphDataTypeEntity>> ListOrderedByTechnicalNameAsync(
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

    public async Task<IReadOnlyDictionary<string, GraphDataTypeEntity>> GetByTechnicalNameAsync(
        CancellationToken cancellationToken)
    {
        return await Set.ToDictionaryAsync(type => type.TechnicalName, cancellationToken);
    }
}
