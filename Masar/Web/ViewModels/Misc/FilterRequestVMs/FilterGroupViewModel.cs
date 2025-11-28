namespace Web.ViewModels.Misc.FilterRequestVMs;

public abstract class FilterGroupViewModel
{
    public string Title { get; set; }
    public string RequestKey { get; set; }
    

    // [Optional]
    public string UiType { get; set; }
}
