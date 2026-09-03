namespace LabInsight.Api.Entities;

public class DashboardWidgetEntity : EntityBase
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public int VisualizationTypeId { get; set; }
    public int MetricDefinitionId { get; set; }
    public int Ordering { get; set; }

    public VisualizationTypeEntity VisualizationType { get; set; } = null!;
    public MetricDefinitionEntity MetricDefinition { get; set; } = null!;
}
