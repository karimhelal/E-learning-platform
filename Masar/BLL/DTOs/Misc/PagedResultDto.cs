namespace BLL.DTOs.Misc;

public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public PaginationSettingsDto Settings { get; set; }
}

public class PaginationSettingsDto
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
