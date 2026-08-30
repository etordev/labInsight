using LabInsight.Api.Entities;

namespace LabInsight.Api.Repositories;

public interface IAnalysisCategoryRepository : IRepository<AnalysisCategory>
{
    Task<IReadOnlyList<AnalysisCategory>> ListOrderedByNameAsync(
        bool isDeleted,
        CancellationToken cancellationToken);
}
