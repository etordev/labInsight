namespace LabInsight.Api.Entities;

public class GraphTypeEntity
{
    public int Id { get; set; }
    public required string TechnicalName { get; set; }

    public ICollection<GraphItemEntity> GraphItems { get; set; } = new List<GraphItemEntity>();
}
