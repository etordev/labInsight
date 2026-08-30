using System.ComponentModel.DataAnnotations;

namespace LabInsight.Api.DTOs;

public class UpsertGraphItemRequest
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int GraphTypeId { get; set; }

    [Range(1, int.MaxValue)]
    public int GraphDataTypeId { get; set; }

    public string? Content { get; set; }
}
