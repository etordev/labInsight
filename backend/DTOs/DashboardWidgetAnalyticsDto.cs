namespace LabInsight.Api.DTOs;

public class AnalyticsPointDto
{
    public required string Label { get; set; }
    public double Value { get; set; }
}

public class DelayedAnalysisRowDto
{
    public required string AnalysisNumber { get; set; }
    public required string Laboratory { get; set; }
    public required string Category { get; set; }
    public DateTime ReceivedAt { get; set; }
    public required string Priority { get; set; }
    public required string Status { get; set; }
    public decimal ExpectedProcessingHours { get; set; }
    public double ElapsedProcessingHours { get; set; }
}

public class DashboardWidgetAnalyticsDto
{
    public required string MetricDefinition { get; set; }
    public required string Unit { get; set; }
    public IReadOnlyList<AnalyticsPointDto> Data { get; set; } = [];
    public IReadOnlyList<DelayedAnalysisRowDto>? Rows { get; set; }
}
