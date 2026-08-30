using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface IAnalysisCategoryService
{
    Task<IReadOnlyList<AnalysisCategoryDto>> GetAnalysisCategoriesAsync(
        bool isDeleted,
        CancellationToken cancellationToken);
}
