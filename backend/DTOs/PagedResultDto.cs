namespace LabInsight.Api.DTOs;

public class PagedResultDto<T>
{
    public required IReadOnlyList<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
