using System.ComponentModel.DataAnnotations;

namespace LabInsight.Api.DTOs;

public class UpdateGraphOrderingItem
{
    [Range(1, int.MaxValue)]
    public int GraphId { get; set; }

    [Range(1, int.MaxValue)]
    public int Ordering { get; set; }
}
