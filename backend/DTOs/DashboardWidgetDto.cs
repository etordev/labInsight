namespace LabInsight.Api.DTOs;

public class DashboardWidgetDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public int VisualizationTypeId { get; set; }
    public int MetricDefinitionId { get; set; }
    public int Ordering { get; set; }
    public required VisualizationTypeDto VisualizationType { get; set; }
    public required MetricDefinitionDto MetricDefinition { get; set; }
}
