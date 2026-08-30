using LabInsight.Api.Enums;

namespace LabInsight.Api.DTOs;

public class LabAnalysisQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? LaboratoryId { get; set; }
    public int? AnalysisCategoryId { get; set; }
    public AnalysisStatus? Status { get; set; }
    public AnalysisPriority? Priority { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
