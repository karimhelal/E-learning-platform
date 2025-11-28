namespace BLL.DTOs.Misc;

public class BrowseRequestDto
{
    public FilterGroupsDto FilterGroups { get; set; }
    public PagingRequestDto PagingRequest { get; set; }
}

public class FilterGroupsDto
{
    // Checkbox Groups
    public List<string>? CategoryNames { get; set; }
    public List<string>? LanguageNames { get; set; }
    public List<string>? LevelNames { get; set; }

    // Range Groups
    public double? MinDuration { get; set; }
    public double? MaxDuration { get; set; }
    public int? MinEnrollments { get; set; }
    public int? MaxEnrollments { get; set; }
    public float? MaxRating { get; set; }
    public float? MinRating { get; set; }
    public DateOnly? MinCreationDate { get; set; }
    public DateOnly? MaxCreationDate { get; set; }

    // Toggle Groups
    public bool? HasCertificate { get; set; }
}