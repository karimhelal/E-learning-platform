namespace BLL.DTOs.Misc;

public class BrowseResultDto<T>
{
    public IEnumerable<T> Items { get; set; }
    public BrowseSettingsDto Settings { get; set; }
}

public class BrowseSettingsDto
{
    public FilterGroupsDto FilterGroups { get; set; }
    public PaginationSettingsDto PaginationSettings { get; set; }
}