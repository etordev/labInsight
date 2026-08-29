using LabInsight.Api.Data;
using LabInsight.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Services;

public class LaboratoryService(LabInsightDbContext dbContext) : ILaboratoryService
{
    public async Task<IReadOnlyList<LaboratoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Laboratories
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .Select(l => new LaboratoryDto
            {
                Id = l.Id,
                Name = l.Name,
                City = l.City
            })
            .ToListAsync(cancellationToken);
    }
}
