using System.ComponentModel.DataAnnotations;

namespace LabInsight.Api.DTOs;

public class UpsertDashboardWidgetRequest
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int VisualizationTypeId { get; set; }

    [Range(1, int.MaxValue)]
    public int MetricDefinitionId { get; set; }

    public string? Content { get; set; }
}
