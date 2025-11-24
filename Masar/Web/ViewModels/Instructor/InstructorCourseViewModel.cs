using Core.Entities;
using Core.Entities.Enums;

namespace Web.ViewModels.Instructor;

public class InstructorCourseViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ThumbnailImageUrl { get; set; }
    public string Status { get; set; }
    public string CreatedDate { get; set; }
    public string LastUpdatedDate { get; set; }
    public string Level { get; set; }
    public string MainCategory { get; set; }


    public int NumberOfStudents { get; set; }
    public int NumberOfModules { get; set; }
    public int NumberOfMinutes { get; set; }
    public int NumberOfHours => Math.Max(1, NumberOfMinutes / 60);
    public float AverageRating { get; set; }

}
