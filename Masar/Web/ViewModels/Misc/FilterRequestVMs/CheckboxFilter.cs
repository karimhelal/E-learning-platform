namespace Web.ViewModels.Misc.FilterRequestVMs;

public class CheckboxFilter : FilterGroupViewModel
{
    public IEnumerable<FilterOption> FilterOptions { get; set; }

    public CheckboxFilter() => UiType = "Checkbox";
}

public class FilterOption
{
    public string Label { get; set; }       // For the UI
    public string Value { get; set; }       // Submition value (JS)
    public bool IsChecked { get; set; }

    // [Optional]
    public int? Count { get; set; }
}
