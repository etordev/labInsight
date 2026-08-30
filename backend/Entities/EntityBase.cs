namespace LabInsight.Api.Entities;

public abstract class EntityBase
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
