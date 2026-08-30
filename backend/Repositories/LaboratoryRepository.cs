using LabInsight.Api.Data;
using LabInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Repositories;

public class LaboratoryRepository(LabInsightDbContext dbContext)
    : Repository<Laboratory>(dbContext), ILaboratoryRepository
{
    public async Task<IReadOnlyList<Laboratory>> ListOrderedByNameAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        return await Query(isDeleted)
            .OrderBy(laboratory => laboratory.Name)
            .ToListAsync(cancellationToken);
    }
}
