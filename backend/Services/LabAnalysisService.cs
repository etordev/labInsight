using LabInsight.Api.DTOs;
using LabInsight.Api.Repositories;

namespace LabInsight.Api.Services;

public class LabAnalysisService(ILabAnalysisRepository labAnalysisRepository) : ILabAnalysisService
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    public async Task<PagedResultDto<LabAnalysisDto>> GetLabAnalysesAsync(
        LabAnalysisQuery query,
        CancellationToken cancellationToken)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? DefaultPageSize : Math.Min(query.PageSize, MaxPageSize);
        var (totalCount, analyses) = await labAnalysisRepository.GetPagedAsync(
            query,
            page,
            pageSize,
            cancellationToken);

        return new PagedResultDto<LabAnalysisDto>
        {
            Items = analyses
                .Select(analysis => new LabAnalysisDto
                {
                    Id = analysis.Id,
                    AnalysisNumber = analysis.AnalysisNumber,
                    LaboratoryId = analysis.LaboratoryId,
                    AnalysisCategoryId = analysis.AnalysisCategoryId,
                    ReceivedAt = analysis.ReceivedAt,
                    StartedAt = analysis.StartedAt,
                    CompletedAt = analysis.CompletedAt,
                    Status = analysis.Status,
                    Priority = analysis.Priority
                })
                .ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
