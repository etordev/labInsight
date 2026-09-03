namespace LabInsight.Api.Entities;

public class VisualizationTypeEntity : EntityBase
{
    public required string TechnicalName { get; set; }

    public ICollection<DashboardWidgetEntity> DashboardWidgets { get; set; } = new List<DashboardWidgetEntity>();
}
