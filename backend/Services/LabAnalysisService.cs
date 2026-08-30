using LabInsight.Api.Data;
using LabInsight.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Services;

public class LabAnalysisService(LabInsightDbContext dbContext) : ILabAnalysisService
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    public async Task<PagedResultDto<LabAnalysisDto>> GetAnalysesAsync(
        LabAnalysisQuery query,
        CancellationToken cancellationToken)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);

        var analyses = dbContext.LabAnalyses.AsNoTracking();

        if (query.StartDate.HasValue)
        {
            analyses = analyses.Where(a => a.ReceivedAt >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            analyses = analyses.Where(a => a.ReceivedAt <= query.EndDate.Value);
        }

        if (query.LaboratoryId.HasValue)
        {
            analyses = analyses.Where(a => a.LaboratoryId == query.LaboratoryId.Value);
        }

        if (query.AnalysisCategoryId.HasValue)
        {
            analyses = analyses.Where(a => a.AnalysisCategoryId == query.AnalysisCategoryId.Value);
        }

        if (query.Status.HasValue)
        {
            analyses = analyses.Where(a => a.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            analyses = analyses.Where(a => a.Priority == query.Priority.Value);
        }

        var totalCount = await analyses.CountAsync(cancellationToken);

        var items = await analyses
            .OrderByDescending(a => a.ReceivedAt)
            .ThenBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new LabAnalysisDto
            {
                Id = a.Id,
                AnalysisNumber = a.AnalysisNumber,
                LaboratoryId = a.LaboratoryId,
                AnalysisCategoryId = a.AnalysisCategoryId,
                ReceivedAt = a.ReceivedAt,
                StartedAt = a.StartedAt,
                CompletedAt = a.CompletedAt,
                Status = a.Status,
                Priority = a.Priority
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<LabAnalysisDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
