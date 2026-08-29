using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface ILabAnalysisService
{
    Task<PagedResultDto<LabAnalysisDto>> GetAsync(LabAnalysisQuery query, CancellationToken cancellationToken);
}
