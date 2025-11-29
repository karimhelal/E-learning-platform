namespace BLL.DTOs.Misc;

public class PagingRequestDto
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 6;

    public string SortBy { get; set; } = "creationDate";
    public string SortOrder { get; set; } = "DESC";
}
