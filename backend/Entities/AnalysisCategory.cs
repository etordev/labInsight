namespace LabInsight.Api.Entities;

public class AnalysisCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required decimal ExpectedProcessingHours { get; set; }

    public ICollection<LabAnalysis> Analyses { get; set; } = new List<LabAnalysis>();
}
