using LabInsight.Api.Data;
using LabInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Repositories;

public class AnalysisCategoryRepository(LabInsightDbContext dbContext)
    : Repository<AnalysisCategory>(dbContext), IAnalysisCategoryRepository
{
    public async Task<IReadOnlyList<AnalysisCategory>> ListOrderedByNameAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        return await Query(isDeleted)
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }
}
