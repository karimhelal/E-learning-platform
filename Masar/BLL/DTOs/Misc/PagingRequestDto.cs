using Core.Entities.Enums;

namespace BLL.DTOs.Misc;

public class PagingRequestDto
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 6;

    public CourseSortOption SortBy { get; set; } = CourseSortOption.CreationDate;
    public SortOrder SortOrder { get; set; } = SortOrder.Descending;
}
