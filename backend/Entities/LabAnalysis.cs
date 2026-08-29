using LabInsight.Api.Enums;

namespace LabInsight.Api.Entities;

public class LabAnalysis
{
    public int Id { get; set; }
    public required string AnalysisNumber { get; set; }
    public int LaboratoryId { get; set; }
    public int AnalysisCategoryId { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public AnalysisStatus Status { get; set; }
    public AnalysisPriority Priority { get; set; }

    public Laboratory Laboratory { get; set; } = null!;
    public AnalysisCategory AnalysisCategory { get; set; } = null!;
}
