namespace Web.ViewModels.Misc.FilterRequestVMs;

public class NumberRangeFilter : FilterGroupViewModel
{
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string Unit { get; set; }

    // For HTML input
    public double Step { get; set; } = 1;

    // [Optional] For dual-handle slider keys
    public string MinRequestKey { get; set; } // e.g., "MinDuration"
    public string MaxRequestKey { get; set; } // e.g., "MaxDuration"

    public NumberRangeFilter() => UiType = "Number Range";
}
