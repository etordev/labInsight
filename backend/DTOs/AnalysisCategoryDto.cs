namespace LabInsight.Api.DTOs;

public class AnalysisCategoryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal ExpectedProcessingHours { get; set; }
}
