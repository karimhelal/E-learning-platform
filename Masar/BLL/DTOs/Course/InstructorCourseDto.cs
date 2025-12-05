using Core.Entities.Enums;

namespace BLL.DTOs.Course;

public class InstructorCourseDto
{
    public int CourseId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailImageUrl { get; set; }
    public string? Status { get; set; }
    public DateOnly? CreatedDate { get; set; }
    public DateOnly? LastUpdatedDate { get; set; }
    public CourseLevel Level { get; set; }
    public string? MainCategory { get; set; }


    public int NumberOfStudents { get; set; }
    public int NumberOfModules { get; set; }
    public int NumberOfMinutes { get; set; }
    public float AverageRating { get; set; }
}
