namespace LabInsight.Api.DTOs;

public class DashboardWidgetContentPayload
{
    public DashboardWidgetFilterPayload? Filters { get; set; }
    public string? GroupBy { get; set; }
}

public class DashboardWidgetFilterPayload
{
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
    public int? LaboratoryId { get; set; }
    public int? AnalysisCategoryId { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
}
