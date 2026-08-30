using System.Globalization;
using System.Text.Json;
using LabInsight.Api.DTOs;
using LabInsight.Api.Entities;
using LabInsight.Api.Enums;
using LabInsight.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LabInsight.Api.Services;

public class AnalyticsService(
    IGraphItemRepository graphItemRepository,
    ILabAnalysisRepository labAnalysisRepository) : IAnalyticsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<GraphItemAnalyticsDto?> GetGraphItemDataAsync(
        int graphItemId,
        bool isDeleted,
        CancellationToken cancellationToken)
    {
        var item = await graphItemRepository.GetWithTypesAsync(graphItemId, isDeleted, cancellationToken);
        if (item is null)
        {
            return null;
        }

        var content = DeserializeContent(item.Content);
        var query = ApplyFilters(labAnalysisRepository.QueryForAnalytics(isDeleted), content);
        var dataType = item.GraphDataType.TechnicalName;
        var graphType = item.GraphType.TechnicalName;

        return dataType switch
        {
            "ANALYSIS_VOLUME" => await GetAnalysisVolumeAsync(query, content, cancellationToken),
            "ANALYSIS_STATUS" => await GetAnalysisStatusAsync(query, cancellationToken),
            "PROCESSING_TIME" => await GetProcessingTimeAsync(query, content, cancellationToken),
            "ANALYSIS_CATEGORY" => await GetAnalysisCategoryAsync(query, cancellationToken),
            "LABORATORY_WORKLOAD" => await GetLaboratoryWorkloadAsync(query, cancellationToken),
            "PRIORITY_DISTRIBUTION" => await GetPriorityDistributionAsync(query, cancellationToken),
            "COMPLETION_RATE" => await GetCompletionRateAsync(query, cancellationToken),
            "DELAYED_ANALYSES" => await GetDelayedAnalysesAsync(query, graphType, cancellationToken),
            _ => new GraphItemAnalyticsDto
            {
                GraphDataType = dataType,
                Unit = "analyses",
                Data = []
            }
        };
    }

    private static GraphItemContentPayload DeserializeContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new GraphItemContentPayload();
        }

        try
        {
            return JsonSerializer.Deserialize<GraphItemContentPayload>(content, JsonOptions)
                   ?? new GraphItemContentPayload();
        }
        catch (JsonException)
        {
            return new GraphItemContentPayload();
        }
    }

    private static IQueryable<LabAnalysis> ApplyFilters(
        IQueryable<LabAnalysis> query,
        GraphItemContentPayload content)
    {
        var filters = content.Filters;
        if (filters is null)
        {
            return query;
        }

        if (TryParseDate(filters.DateFrom, endOfDay: false, out var dateFrom))
        {
            query = query.Where(analysis => analysis.ReceivedAt >= dateFrom);
        }

        if (TryParseDate(filters.DateTo, endOfDay: true, out var dateTo))
        {
            query = query.Where(analysis => analysis.ReceivedAt <= dateTo);
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

    private static bool TryParseDate(string? value, bool endOfDay, out DateTime parsed)
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

        parsed = endOfDay
            ? DateTime.SpecifyKind(date.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
            : DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return true;
    }

    private static async Task<GraphItemAnalyticsDto> GetAnalysisVolumeAsync(
        IQueryable<LabAnalysis> query,
        GraphItemContentPayload content,
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

        return new GraphItemAnalyticsDto
        {
            GraphDataType = "ANALYSIS_VOLUME",
            Unit = "analyses",
            Data = points
        };
    }

    private static async Task<GraphItemAnalyticsDto> GetAnalysisStatusAsync(
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

        return new GraphItemAnalyticsDto
        {
            GraphDataType = "ANALYSIS_STATUS",
            Unit = "analyses",
            Data = points
        };
    }

    private static async Task<GraphItemAnalyticsDto> GetProcessingTimeAsync(
        IQueryable<LabAnalysis> query,
        GraphItemContentPayload content,
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

        return new GraphItemAnalyticsDto
        {
            GraphDataType = "PROCESSING_TIME",
            Unit = "hours",
            Data = points
        };
    }

    private static async Task<GraphItemAnalyticsDto> GetAnalysisCategoryAsync(
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

        return new GraphItemAnalyticsDto
        {
            GraphDataType = "ANALYSIS_CATEGORY",
            Unit = "analyses",
            Data = rows
        };
    }

    private static async Task<GraphItemAnalyticsDto> GetLaboratoryWorkloadAsync(
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

        return new GraphItemAnalyticsDto
        {
            GraphDataType = "LABORATORY_WORKLOAD",
            Unit = "analyses",
            Data = rows
        };
    }

    private static async Task<GraphItemAnalyticsDto> GetPriorityDistributionAsync(
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

        return new GraphItemAnalyticsDto
        {
            GraphDataType = "PRIORITY_DISTRIBUTION",
            Unit = "analyses",
            Data = points
        };
    }

    private static async Task<GraphItemAnalyticsDto> GetCompletionRateAsync(
        IQueryable<LabAnalysis> query,
        CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        if (total == 0)
        {
            return new GraphItemAnalyticsDto
            {
                GraphDataType = "COMPLETION_RATE",
                Unit = "percent",
                Data = []
            };
        }

        var completed = await query.CountAsync(
            analysis => analysis.Status == AnalysisStatus.Completed,
            cancellationToken);
        var completedPercent = Math.Round(completed * 100.0 / total, 1);
        var remaining = Math.Round(100.0 - completedPercent, 1);

        return new GraphItemAnalyticsDto
        {
            GraphDataType = "COMPLETION_RATE",
            Unit = "percent",
            Data =
            [
                new AnalyticsPointDto { Label = "Completed", Value = completedPercent },
                new AnalyticsPointDto { Label = "Not Completed", Value = remaining }
            ]
        };
    }

    private static async Task<GraphItemAnalyticsDto> GetDelayedAnalysesAsync(
        IQueryable<LabAnalysis> query,
        string graphType,
        CancellationToken cancellationToken)
    {
        var delayed = query.Where(analysis => analysis.Status == AnalysisStatus.Delayed);

        if (graphType == "DATA_GRID")
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

            return new GraphItemAnalyticsDto
            {
                GraphDataType = "DELAYED_ANALYSES",
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

        return new GraphItemAnalyticsDto
        {
            GraphDataType = "DELAYED_ANALYSES",
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
