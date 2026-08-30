using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface ILabAnalysisService
{
    Task<PagedResultDto<LabAnalysisDto>> GetAnalysesAsync(LabAnalysisQuery query, CancellationToken cancellationToken);
}
