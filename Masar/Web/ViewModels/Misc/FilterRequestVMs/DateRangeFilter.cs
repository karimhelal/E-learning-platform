namespace Web.ViewModels.Misc.FilterRequestVMs;

public class DateRangeFilter : FilterGroupViewModel
{
    public DateOnly? MinDate { get; set; }
    public DateOnly? MaxDate { get; set; }


    // [Optional] For dual-handle slider keys
    public string MinRequestKey { get; set; } // e.g., "MinDate"
    public string MaxRequestKey { get; set; } // e.g., "MaxDate"

    public DateRangeFilter() => UiType = "Date Range";
}
