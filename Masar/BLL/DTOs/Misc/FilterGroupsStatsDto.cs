namespace BLL.DTOs.Misc;

public class FilterGroupsStatsDto
{
    public Dictionary<string, int> CategoryCounts { get; set; }
    public Dictionary<string, int> LevelCounts { get; set; }
    public Dictionary<string, int> LanguageCounts { get; set; }
}
