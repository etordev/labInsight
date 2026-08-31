namespace LabInsight.Api.Entities;

public class GraphItemEntity : EntityBase
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public int GraphTypeId { get; set; }
    public int GraphDataTypeId { get; set; }
    public int Ordering { get; set; }

    public GraphTypeEntity GraphType { get; set; } = null!;
    public GraphDataTypeEntity GraphDataType { get; set; } = null!;
}
