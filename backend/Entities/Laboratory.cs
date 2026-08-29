namespace LabInsight.Api.Entities;

public class Laboratory
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }

    public ICollection<LabAnalysis> Analyses { get; set; } = new List<LabAnalysis>();
}
