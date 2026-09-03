using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface IAnalyticsService
{
    Task<DashboardWidgetAnalyticsDto?> GetDashboardWidgetDataAsync(
        int dashboardWidgetId,
        bool isDeleted,
        CancellationToken cancellationToken);
}
