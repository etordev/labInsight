using LabInsight.Api.DTOs;
using LabInsight.Api.Entities;

namespace LabInsight.Api.Repositories;

public interface ILabAnalysisRepository : IRepository<LabAnalysis>
{
    IQueryable<LabAnalysis> QueryForAnalytics(bool isDeleted);

    Task<(int TotalCount, IReadOnlyList<LabAnalysis> Items)> GetPagedAsync(
        LabAnalysisQuery query,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task DeleteAllAsync(CancellationToken cancellationToken);
}
