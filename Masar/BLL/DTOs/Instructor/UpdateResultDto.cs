namespace BLL.DTOs.Instructor;

public class UpdateResultDto
{
    public bool Success { get; set; }
    public int? ModuleId { get; set; }
    public int? LessonId { get; set; }
    public string? Message { get; set; }
}