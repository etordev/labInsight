using LabInsight.Api.Data;
using LabInsight.Api.DTOs;
using LabInsight.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Repositories;

public class LabAnalysisRepository(LabInsightDbContext dbContext)
    : Repository<LabAnalysis>(dbContext), ILabAnalysisRepository
{
    public IQueryable<LabAnalysis> QueryForAnalytics(bool isDeleted)
    {
        return Query(isDeleted);
    }

    public async Task<(int TotalCount, IReadOnlyList<LabAnalysis> Items)> GetPagedAsync(
        LabAnalysisQuery query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var analyses = Query(query.IsDeleted);

        if (query.StartDate.HasValue)
        {
            analyses = analyses.Where(analysis => analysis.ReceivedAt >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            analyses = analyses.Where(analysis => analysis.ReceivedAt <= query.EndDate.Value);
        }

        if (query.LaboratoryId.HasValue)
        {
            analyses = analyses.Where(analysis => analysis.LaboratoryId == query.LaboratoryId.Value);
        }

        if (query.AnalysisCategoryId.HasValue)
        {
            analyses = analyses.Where(analysis =>
                analysis.AnalysisCategoryId == query.AnalysisCategoryId.Value);
        }

        if (query.Status.HasValue)
        {
            analyses = analyses.Where(analysis => analysis.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            analyses = analyses.Where(analysis => analysis.Priority == query.Priority.Value);
        }

        var totalCount = await analyses.CountAsync(cancellationToken);
        var items = await analyses
            .OrderByDescending(analysis => analysis.ReceivedAt)
            .ThenBy(analysis => analysis.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (totalCount, items);
    }

    public Task DeleteAllAsync(CancellationToken cancellationToken)
    {
        return Set.ExecuteDeleteAsync(cancellationToken);
    }
}
