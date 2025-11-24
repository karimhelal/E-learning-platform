namespace Web.ViewModels.Instructor;

public class PagedResultViewModel<T>
{
    public IEnumerable<T> Items { get; set; }
    public PaginationSettingsViewModel Settings { get; set; }
}
