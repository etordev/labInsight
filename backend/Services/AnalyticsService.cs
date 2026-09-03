using System.Globalization;
using System.Text.Json;
using LabInsight.Api.Catalog;
using LabInsight.Api.DTOs;
using LabInsight.Api.Entities;
using LabInsight.Api.Enums;
using LabInsight.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Services;

public class AnalyticsService(
    IDashboardWidgetRepository dashboardWidgetRepository,
    ILabAnalysisRepository labAnalysisRepository) : IAnalyticsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<DashboardWidgetAnalyticsDto?> GetDashboardWidgetDataAsync(
        int dashboardWidgetId,
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        var item = await dashboardWidgetRepository.GetWithTypesAsync(dashboardWidgetId, isDeleted, cancellationToken);
        if (item is null)
        {
            return null;
        }

        var content = DeserializeContent(item.Content);
        var query = ApplyFilters(
            labAnalysisRepository.QueryForAnalytics(isDeleted),
            content,
            item.MetricDefinition.TechnicalName);
        var dataType = item.MetricDefinition.TechnicalName;
        var visualizationType = item.VisualizationType.TechnicalName;

        return dataType switch
        {
            "ANALYSIS_VOLUME" => await GetAnalysisVolumeAsync(query, content, cancellationToken),
            "ANALYSIS_STATUS" => await GetAnalysisStatusAsync(query, cancellationToken),
            "PROCESSING_TIME" => await GetProcessingTimeAsync(query, content, cancellationToken),
            "ANALYSIS_CATEGORY" => await GetAnalysisCategoryAsync(query, cancellationToken),
            "LABORATORY_WORKLOAD" => await GetLaboratoryWorkloadAsync(query, cancellationToken),
            "PRIORITY_DISTRIBUTION" => await GetPriorityDistributionAsync(query, cancellationToken),
            "COMPLETION_RATE" => await GetCompletionRateAsync(query, cancellationToken),
            "DELAYED_ANALYSES" => await GetDelayedAnalysesAsync(query, visualizationType, cancellationToken),
            _ => new DashboardWidgetAnalyticsDto
            {
                MetricDefinition = dataType,
                Unit = "analyses",
                Data = []
            }
        };
    }

    private static DashboardWidgetContentPayload DeserializeContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new DashboardWidgetContentPayload();
        }

        try
        {
            return JsonSerializer.Deserialize<DashboardWidgetContentPayload>(content, JsonOptions)
                   ?? new DashboardWidgetContentPayload();
        }
        catch (JsonException)
        {
            return new DashboardWidgetContentPayload();
        }
    }

    private static IQueryable<LabAnalysis> ApplyFilters(
        IQueryable<LabAnalysis> query,
        DashboardWidgetContentPayload content,
        string metricDefinition)
    {
        var filters = content.Filters;
        query = ApplyReceivedAtDateRange(query, filters, metricDefinition);

        if (filters is null)
        {
            return query;
        }

        if (filters.LaboratoryId is > 0)
        {
            query = query.Where(analysis => analysis.LaboratoryId == filters.LaboratoryId);
        }

        if (filters.AnalysisCategoryId is > 0)
        {
            query = query.Where(analysis => analysis.AnalysisCategoryId == filters.AnalysisCategoryId);
        }

        if (Enum.TryParse<AnalysisPriority>(filters.Priority, ignoreCase: true, out var priority))
        {
            query = query.Where(analysis => analysis.Priority == priority);
        }

        if (Enum.TryParse<AnalysisStatus>(filters.Status, ignoreCase: true, out var status))
        {
            query = query.Where(analysis => analysis.Status == status);
        }

        return query;
    }

    private static IQueryable<LabAnalysis> ApplyReceivedAtDateRange(
        IQueryable<LabAnalysis> query,
        DashboardWidgetFilterPayload? filters,
        string metricDefinition)
    {
        if (!DashboardCatalog.SupportsDateRange(metricDefinition))
        {
            return query;
        }

        var hasFrom = TryParseDate(filters?.DateFrom, out var dateFrom);
        var hasTo = TryParseDate(filters?.DateTo, out var dateTo);

        if (!hasFrom && !hasTo)
        {
            var todayUtc = DateTime.UtcNow.Date;
            dateFrom = DateTime.SpecifyKind(todayUtc.AddMonths(-12), DateTimeKind.Utc);
            dateTo = DateTime.SpecifyKind(todayUtc, DateTimeKind.Utc);
            hasFrom = true;
            hasTo = true;
        }

        if (hasFrom)
        {
            dateFrom = DateTime.SpecifyKind(dateFrom.Date, DateTimeKind.Utc);
            query = query.Where(analysis => analysis.ReceivedAt >= dateFrom);
        }

        if (hasTo)
        {
            var dateToEnd = DateTime.SpecifyKind(dateTo.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(analysis => analysis.ReceivedAt <= dateToEnd);
        }

        return query;
    }

    private static bool TryParseDate(string? value, out DateTime parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
        {
            return false;
        }

        parsed = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return true;
    }

    private static async Task<DashboardWidgetAnalyticsDto> GetAnalysisVolumeAsync(
        IQueryable<LabAnalysis> query,
        DashboardWidgetContentPayload content,
        CancellationToken cancellationToken)
    {
        var groupBy = (content.GroupBy ?? "MONTH").ToUpperInvariant();
        List<AnalyticsPointDto> points;

        if (groupBy == "DAY")
        {
            var rows = await query
                .GroupBy(analysis => analysis.ReceivedAt.Date)
                .Select(group => new { Period = group.Key, Value = group.Count() })
                .OrderBy(row => row.Period)
                .ToListAsync(cancellationToken);

            points = rows
                .Select(row => new AnalyticsPointDto
                {
                    Label = DateTime.SpecifyKind(row.Period, DateTimeKind.Utc).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Value = row.Value
                })
                .ToList();
        }
        else if (groupBy == "WEEK")
        {
            var rows = await query
                .GroupBy(analysis => analysis.ReceivedAt.AddDays(-((((int)analysis.ReceivedAt.DayOfWeek) + 6) % 7)).Date)
                .Select(group => new { Period = group.Key, Value = group.Count() })
                .OrderBy(row => row.Period)
                .ToListAsync(cancellationToken);

            points = rows
                .Select(row => new AnalyticsPointDto
                {
                    Label = IsoWeekLabel(row.Period),
                    Value = row.Value
                })
                .ToList();
        }
        else
        {
            var rows = await query
                .GroupBy(analysis => new { analysis.ReceivedAt.Year, analysis.ReceivedAt.Month })
                .Select(group => new { group.Key.Year, group.Key.Month, Value = group.Count() })
                .OrderBy(row => row.Year)
                .ThenBy(row => row.Month)
                .ToListAsync(cancellationToken);

            points = rows
                .Select(row => new AnalyticsPointDto
                {
                    Label = $"{row.Year:D4}-{row.Month:D2}",
                    Value = row.Value
                })
                .ToList();
        }

        return new DashboardWidgetAnalyticsDto
        {
            MetricDefinition = "ANALYSIS_VOLUME",
            Unit = "analyses",
            Data = points
        };
    }

    private static async Task<DashboardWidgetAnalyticsDto> GetAnalysisStatusAsync(
        IQueryable<LabAnalysis> query,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .GroupBy(analysis => analysis.Status)
            .Select(group => new { Status = group.Key, Value = group.Count() })
            .ToListAsync(cancellationToken);

        var order = new[]
        {
            AnalysisStatus.Completed,
            AnalysisStatus.Processing,
            AnalysisStatus.Pending,
            AnalysisStatus.Delayed,
            AnalysisStatus.Cancelled
        };

        var lookup = rows.ToDictionary(row => row.Status, row => row.Value);
        var points = order
            .Select(status => new AnalyticsPointDto
            {
                Label = status.ToString(),
                Value = lookup.GetValueOrDefault(status)
            })
            .Where(point => point.Value > 0)
            .ToList();

        return new DashboardWidgetAnalyticsDto
        {
            MetricDefinition = "ANALYSIS_STATUS",
            Unit = "analyses",
            Data = points
        };
    }

    private static async Task<DashboardWidgetAnalyticsDto> GetProcessingTimeAsync(
        IQueryable<LabAnalysis> query,
        DashboardWidgetContentPayload content,
        CancellationToken cancellationToken)
    {
        var completed = query.Where(analysis =>
            analysis.Status == AnalysisStatus.Completed
            && analysis.StartedAt != null
            && analysis.CompletedAt != null);

        var groupBy = content.GroupBy?.ToUpperInvariant();
        List<AnalyticsPointDto> points;

        if (groupBy is "DAY" or "WEEK" or "MONTH")
        {
            if (groupBy == "DAY")
            {
                var rows = await completed
                    .GroupBy(analysis => analysis.ReceivedAt.Date)
                    .Select(group => new
                    {
                        Period = group.Key,
                        Value = group.Average(analysis =>
                            (analysis.CompletedAt!.Value - analysis.StartedAt!.Value).TotalHours)
                    })
                    .OrderBy(row => row.Period)
                    .ToListAsync(cancellationToken);

                points = rows
                    .Select(row => new AnalyticsPointDto
                    {
                        Label = DateTime.SpecifyKind(row.Period, DateTimeKind.Utc)
                            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        Value = Math.Round(row.Value, 1)
                    })
                    .ToList();
            }
            else if (groupBy == "WEEK")
            {
                var rows = await completed
                    .GroupBy(analysis => analysis.ReceivedAt.AddDays(-((((int)analysis.ReceivedAt.DayOfWeek) + 6) % 7)).Date)
                    .Select(group => new
                    {
                        Period = group.Key,
                        Value = group.Average(analysis =>
                            (analysis.CompletedAt!.Value - analysis.StartedAt!.Value).TotalHours)
                    })
                    .OrderBy(row => row.Period)
                    .ToListAsync(cancellationToken);

                points = rows
                    .Select(row => new AnalyticsPointDto
                    {
                        Label = IsoWeekLabel(row.Period),
                        Value = Math.Round(row.Value, 1)
                    })
                    .ToList();
            }
            else
            {
                var rows = await completed
                    .GroupBy(analysis => new { analysis.ReceivedAt.Year, analysis.ReceivedAt.Month })
                    .Select(group => new
                    {
                        group.Key.Year,
                        group.Key.Month,
                        Value = group.Average(analysis =>
                            (analysis.CompletedAt!.Value - analysis.StartedAt!.Value).TotalHours)
                    })
                    .OrderBy(row => row.Year)
                    .ThenBy(row => row.Month)
                    .ToListAsync(cancellationToken);

                points = rows
                    .Select(row => new AnalyticsPointDto
                    {
                        Label = $"{row.Year:D4}-{row.Month:D2}",
                        Value = Math.Round(row.Value, 1)
                    })
                    .ToList();
            }
        }
        else
        {
            var rows = await completed
                .GroupBy(analysis => analysis.AnalysisCategory.Name)
                .Select(group => new
                {
                    Label = group.Key,
                    Value = group.Average(analysis =>
                        (analysis.CompletedAt!.Value - analysis.StartedAt!.Value).TotalHours)
                })
                .OrderBy(row => row.Label)
                .ToListAsync(cancellationToken);

            points = rows
                .Select(row => new AnalyticsPointDto
                {
                    Label = row.Label,
                    Value = Math.Round(row.Value, 1)
                })
                .ToList();
        }

        return new DashboardWidgetAnalyticsDto
        {
            MetricDefinition = "PROCESSING_TIME",
            Unit = "hours",
            Data = points
        };
    }

    private static async Task<DashboardWidgetAnalyticsDto> GetAnalysisCategoryAsync(
        IQueryable<LabAnalysis> query,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .GroupBy(analysis => analysis.AnalysisCategory.Name)
            .Select(group => new AnalyticsPointDto
            {
                Label = group.Key,
                Value = group.Count()
            })
            .OrderBy(row => row.Label)
            .ToListAsync(cancellationToken);

        return new DashboardWidgetAnalyticsDto
        {
            MetricDefinition = "ANALYSIS_CATEGORY",
            Unit = "analyses",
            Data = rows
        };
    }

    private static async Task<DashboardWidgetAnalyticsDto> GetLaboratoryWorkloadAsync(
        IQueryable<LabAnalysis> query,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .Where(analysis =>
                analysis.Status == AnalysisStatus.Pending
                || analysis.Status == AnalysisStatus.Processing
                || analysis.Status == AnalysisStatus.Delayed)
            .GroupBy(analysis => analysis.Laboratory.Name)
            .Select(group => new AnalyticsPointDto
            {
                Label = group.Key,
                Value = group.Count()
            })
            .OrderBy(row => row.Label)
            .ToListAsync(cancellationToken);

        return new DashboardWidgetAnalyticsDto
        {
            MetricDefinition = "LABORATORY_WORKLOAD",
            Unit = "analyses",
            Data = rows
        };
    }

    private static async Task<DashboardWidgetAnalyticsDto> GetPriorityDistributionAsync(
        IQueryable<LabAnalysis> query,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .GroupBy(analysis => analysis.Priority)
            .Select(group => new { Priority = group.Key, Value = group.Count() })
            .ToListAsync(cancellationToken);

        var order = new[] { AnalysisPriority.Normal, AnalysisPriority.High, AnalysisPriority.Urgent };
        var lookup = rows.ToDictionary(row => row.Priority, row => row.Value);
        var points = order
            .Select(priority => new AnalyticsPointDto
            {
                Label = priority.ToString(),
                Value = lookup.GetValueOrDefault(priority)
            })
            .Where(point => point.Value > 0)
            .ToList();

        return new DashboardWidgetAnalyticsDto
        {
            MetricDefinition = "PRIORITY_DISTRIBUTION",
            Unit = "analyses",
            Data = points
        };
    }

    private static async Task<DashboardWidgetAnalyticsDto> GetCompletionRateAsync(
        IQueryable<LabAnalysis> query,
        CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        if (total == 0)
        {
            return new DashboardWidgetAnalyticsDto
            {
                MetricDefinition = "COMPLETION_RATE",
                Unit = "percent",
                Data = []
            };
        }

        var completed = await query.CountAsync(
            analysis => analysis.Status == AnalysisStatus.Completed,
            cancellationToken);
        var completedPercent = Math.Round(completed * 100.0 / total, 1);
        var remaining = Math.Round(100.0 - completedPercent, 1);

        return new DashboardWidgetAnalyticsDto
        {
            MetricDefinition = "COMPLETION_RATE",
            Unit = "percent",
            Data =
            [
                new AnalyticsPointDto { Label = "Completed", Value = completedPercent },
                new AnalyticsPointDto { Label = "Not Completed", Value = remaining }
            ]
        };
    }

    private static async Task<DashboardWidgetAnalyticsDto> GetDelayedAnalysesAsync(
        IQueryable<LabAnalysis> query,
        string visualizationType,
        CancellationToken cancellationToken)
    {
        var delayed = query.Where(analysis => analysis.Status == AnalysisStatus.Delayed);

        if (visualizationType == "DATA_GRID")
        {
            var now = DateTime.UtcNow;
            var rows = await delayed
                .OrderByDescending(analysis => analysis.ReceivedAt)
                .Take(200)
                .Select(analysis => new DelayedAnalysisRowDto
                {
                    AnalysisNumber = analysis.AnalysisNumber,
                    Laboratory = analysis.Laboratory.Name,
                    Category = analysis.AnalysisCategory.Name,
                    ReceivedAt = analysis.ReceivedAt,
                    Priority = analysis.Priority.ToString(),
                    Status = analysis.Status.ToString(),
                    ExpectedProcessingHours = analysis.AnalysisCategory.ExpectedProcessingHours,
                    ElapsedProcessingHours = 0
                })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                row.ElapsedProcessingHours = Math.Round((now - row.ReceivedAt).TotalHours, 1);
            }

            return new DashboardWidgetAnalyticsDto
            {
                MetricDefinition = "DELAYED_ANALYSES",
                Unit = "analyses",
                Data = [],
                Rows = rows
            };
        }

        var points = await delayed
            .GroupBy(analysis => analysis.AnalysisCategory.Name)
            .Select(group => new AnalyticsPointDto
            {
                Label = group.Key,
                Value = group.Count()
            })
            .OrderBy(row => row.Label)
            .ToListAsync(cancellationToken);

        return new DashboardWidgetAnalyticsDto
        {
            MetricDefinition = "DELAYED_ANALYSES",
            Unit = "analyses",
            Data = points
        };
    }

    private static string IsoWeekLabel(DateTime period)
    {
        var utc = DateTime.SpecifyKind(period, DateTimeKind.Utc);
        var week = ISOWeek.GetWeekOfYear(utc);
        return $"{ISOWeek.GetYear(utc)}-W{week:D2}";
    }
}
