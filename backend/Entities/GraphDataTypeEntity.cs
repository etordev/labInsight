namespace LabInsight.Api.Entities;

public class GraphDataTypeEntity : EntityBase
{
    public required string TechnicalName { get; set; }

    public ICollection<GraphItemEntity> GraphItems { get; set; } = new List<GraphItemEntity>();
}
