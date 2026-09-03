using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface ILabAnalysisService
{
    Task<PagedResultDto<LabAnalysisDto>> GetLabAnalysesAsync(LabAnalysisQuery query, CancellationToken cancellationToken);
}
