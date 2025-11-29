namespace Web.ViewModels.Home;

public class BrowseCoursesViewModel
{
    public BrowseResultViewModel<CourseBrowseCardViewModel> Data { get; set; }

    // UI specific properties
    public string PageTitle { get; set; }
}
