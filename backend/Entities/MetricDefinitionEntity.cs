namespace LabInsight.Api.Entities;

public class MetricDefinitionEntity : EntityBase
{
    public required string TechnicalName { get; set; }

    public ICollection<DashboardWidgetEntity> DashboardWidgets { get; set; } = new List<DashboardWidgetEntity>();
}
