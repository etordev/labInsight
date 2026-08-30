using LabInsight.Api.DTOs;
using LabInsight.Api.Repositories;

namespace LabInsight.Api.Services;

public class LaboratoryService(ILaboratoryRepository laboratoryRepository) : ILaboratoryService
{
    public async Task<IReadOnlyList<LaboratoryDto>> GetLaboratoriesAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        var laboratories = await laboratoryRepository.ListOrderedByNameAsync(isDeleted, cancellationToken);
        return laboratories
            .Select(laboratory => new LaboratoryDto
            {
                Id = laboratory.Id,
                Name = laboratory.Name,
                City = laboratory.City
            })
            .ToList();
    }
}
