using Web.ViewModels.Misc;
using Web.ViewModels.Misc.FilterRequestVMs;

namespace Web.ViewModels.Home;

public class BrowseResultViewModel<T>
{
    public IEnumerable<T> Items { get; set; }
    public BrowseSettingsViewModel Settings { get; set; }
}

public class BrowseSettingsViewModel
{
    public IEnumerable<FilterGroupViewModel> FilterGroups { get; set; }
    public PaginationSettingsViewModel PaginationSettings { get; set; }
}
