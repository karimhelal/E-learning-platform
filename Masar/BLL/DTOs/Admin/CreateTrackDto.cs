namespace BLL.DTOs.Admin;

public class CreateTrackDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; } = 0; // Default: Draft
    public List<int> CourseIds { get; set; } = [];
}