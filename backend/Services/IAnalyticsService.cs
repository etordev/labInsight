using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface IAnalyticsService
{
    Task<GraphItemAnalyticsDto?> GetGraphItemDataAsync(
        int graphItemId,
        bool isDeleted,
        CancellationToken cancellationToken);
}
