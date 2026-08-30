using LabInsight.Api.DTOs;
using LabInsight.Api.Repositories;

namespace LabInsight.Api.Services;

public class AnalysisCategoryService(IAnalysisCategoryRepository analysisCategoryRepository)
    : IAnalysisCategoryService
{
    public async Task<IReadOnlyList<AnalysisCategoryDto>> GetAnalysisCategoriesAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        var categories = await analysisCategoryRepository.ListOrderedByNameAsync(
            isDeleted,
            cancellationToken);

        return categories
            .Select(category => new AnalysisCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ExpectedProcessingHours = category.ExpectedProcessingHours
            })
            .ToList();
    }
}
