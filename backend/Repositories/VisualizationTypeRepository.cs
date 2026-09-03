using LabInsight.Api.Data;
using LabInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Repositories;

public class VisualizationTypeRepository(LabInsightDbContext dbContext)
    : Repository<VisualizationTypeEntity>(dbContext), IVisualizationTypeRepository
{
    public async Task<IReadOnlyList<VisualizationTypeEntity>> ListOrderedByTechnicalNameAsync(
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

    public async Task<IReadOnlyDictionary<string, VisualizationTypeEntity>> GetByTechnicalNameAsync(
        CancellationToken cancellationToken)
    {
        return await Set.ToDictionaryAsync(type => type.TechnicalName, cancellationToken);
    }
}
