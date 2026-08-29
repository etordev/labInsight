namespace LabInsight.Api.Entities;

public class GraphItemEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public int GraphTypeId { get; set; }
    public int GraphDataTypeId { get; set; }

    public GraphTypeEntity GraphType { get; set; } = null!;
    public GraphDataTypeEntity GraphDataType { get; set; } = null!;
}
