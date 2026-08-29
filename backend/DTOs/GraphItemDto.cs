namespace LabInsight.Api.DTOs;

public class GraphItemDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public int GraphTypeId { get; set; }
    public int GraphDataTypeId { get; set; }
    public required GraphTypeDto GraphType { get; set; }
    public required GraphDataTypeDto GraphDataType { get; set; }
}
