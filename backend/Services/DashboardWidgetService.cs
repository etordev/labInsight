using LabInsight.Api.DTOs;
using LabInsight.Api.Entities;
using LabInsight.Api.Repositories;

namespace LabInsight.Api.Services;

public class DashboardWidgetService(
    IDashboardWidgetRepository dashboardWidgetRepository,
    IVisualizationTypeRepository visualizationTypeRepository,
    IMetricDefinitionRepository metricDefinitionRepository) : IDashboardWidgetService
{
    public async Task<IReadOnlyList<DashboardWidgetDto>> GetDashboardWidgetsAsync(
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        var items = await dashboardWidgetRepository.ListWithTypesAsync(isDeleted, cancellationToken);
        return items.Select(MapDto).ToList();
    }

    public async Task<UpsertDashboardWidgetResult> UpsertDashboardWidgetAsync(
        UpsertDashboardWidgetRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return new UpsertDashboardWidgetResult(null, "Name is required.", false, false);
        }

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        var content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim();

        var visualizationTypeExists = await visualizationTypeRepository.ExistsAsync(
            request.VisualizationTypeId,
            isDeleted: false,
            cancellationToken);
        if (!visualizationTypeExists)
        {
            return new UpsertDashboardWidgetResult(null, "Visualization type was not found.", true, false);
        }

        var metricDefinitionExists = await metricDefinitionRepository.ExistsAsync(
            request.MetricDefinitionId,
            isDeleted: false,
            cancellationToken);
        if (!metricDefinitionExists)
        {
            return new UpsertDashboardWidgetResult(null, "Metric definition was not found.", true, false);
        }

        DashboardWidgetEntity entity;
        var created = false;

        if (request.Id is > 0)
        {
            var existing = await dashboardWidgetRepository.GetTrackedAsync(
                request.Id.Value,
                isDeleted: false,
                cancellationToken);

            if (existing is null)
            {
                return new UpsertDashboardWidgetResult(null, "Dashboard widget was not found.", true, false);
            }

            entity = existing;
            entity.Name = name;
            entity.Description = description;
            entity.Content = content;
            entity.VisualizationTypeId = request.VisualizationTypeId;
            entity.MetricDefinitionId = request.MetricDefinitionId;
        }
        else
        {
            entity = new DashboardWidgetEntity
            {
                Name = name,
                Description = description,
                Content = content,
                VisualizationTypeId = request.VisualizationTypeId,
                MetricDefinitionId = request.MetricDefinitionId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                Ordering = await dashboardWidgetRepository.GetMaxOrderingAsync(cancellationToken) + 1
            };

            dashboardWidgetRepository.Add(entity);
            created = true;
        }

        await dashboardWidgetRepository.SaveChangesAsync(cancellationToken);
        await dashboardWidgetRepository.LoadTypesAsync(entity, cancellationToken);

        return new UpsertDashboardWidgetResult(MapDto(entity), null, false, created);
    }

    public Task<bool> DeleteDashboardWidgetAsync(int id, CancellationToken cancellationToken)
    {
        return dashboardWidgetRepository.SoftDeleteAsync(id, cancellationToken);
    }

    public async Task<string?> UpdateDashboardWidgetOrderingAsync(
        IReadOnlyList<UpdateDashboardWidgetOrderingItem> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return "At least one dashboard widget is required.";
        }

        var dashboardWidgetIds = items.Select(item => item.DashboardWidgetId).ToList();
        if (dashboardWidgetIds.Distinct().Count() != dashboardWidgetIds.Count)
        {
            return "Each dashboard widget can appear only once.";
        }

        var orderings = items.Select(item => item.Ordering).ToList();
        if (orderings.Any(ordering => ordering < 1) || orderings.Distinct().Count() != orderings.Count)
        {
            return "Ordering values must be unique positive numbers.";
        }

        var existing = await dashboardWidgetRepository.GetTrackedByIdsAsync(dashboardWidgetIds, isDeleted: false, cancellationToken);
        if (existing.Count != dashboardWidgetIds.Count)
        {
            return "One or more dashboard widgets were not found.";
        }

        var orderingById = items.ToDictionary(item => item.DashboardWidgetId, item => item.Ordering);
        foreach (var entity in existing)
        {
            entity.Ordering = orderingById[entity.Id];
        }

        await dashboardWidgetRepository.SaveChangesAsync(cancellationToken);
        return null;
    }

    private static DashboardWidgetDto MapDto(DashboardWidgetEntity item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        Content = item.Content,
        VisualizationTypeId = item.VisualizationTypeId,
        MetricDefinitionId = item.MetricDefinitionId,
        Ordering = item.Ordering,
        VisualizationType = new VisualizationTypeDto
        {
            Id = item.VisualizationType.Id,
            TechnicalName = item.VisualizationType.TechnicalName
        },
        MetricDefinition = new MetricDefinitionDto
        {
            Id = item.MetricDefinition.Id,
            TechnicalName = item.MetricDefinition.TechnicalName
        }
    };
}
