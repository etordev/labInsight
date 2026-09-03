using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface IDashboardWidgetService
{
    Task<IReadOnlyList<DashboardWidgetDto>> GetDashboardWidgetsAsync(bool isDeleted, CancellationToken cancellationToken);

    Task<UpsertDashboardWidgetResult> UpsertDashboardWidgetAsync(
        UpsertDashboardWidgetRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteDashboardWidgetAsync(int id, CancellationToken cancellationToken);

    Task<string?> UpdateDashboardWidgetOrderingAsync(
        IReadOnlyList<UpdateDashboardWidgetOrderingItem> items,
        CancellationToken cancellationToken);
}

public sealed record UpsertDashboardWidgetResult(DashboardWidgetDto? Item, string? Error, bool NotFound, bool Created);
