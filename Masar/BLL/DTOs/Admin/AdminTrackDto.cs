namespace BLL.DTOs.Admin;

public class AdminTrackDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CoursesCount { get; set; }
    public int StudentsCount { get; set; }
    public DateOnly CreatedDate { get; set; }
}