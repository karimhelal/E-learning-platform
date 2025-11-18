using Core.Entities;
using Core.Entities.Enums;

namespace Web.ViewModels.Course;

// Tag
// Title
// Desc
// # students
// # modules
// # hours
// # lessons
// thumbnail

public class CourseViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; }
    public string CourseDescription { get; set; }
    public string ThumbnailUrl { get; set; }
    public string Language { get; set; }
    public CourseLevel Level { get; set; }
    public ICollection<Category> Categories { get; set; }

    public int NumberOfStudents { get; set; }
    public int NumberOfModules { get; set; }
    public int NumberOfLessons { get; set; }
    public int NumberOfAssignments { get; set; }
    public int NumberOfMinutes { get; set; }
    public int NumberOfHours => Math.Max(1, NumberOfMinutes / 60);

    public CourseViewModel(
        int courseId, 
        string courseTitle, 
        string courseDescription, 
        string thumbnailUrl,
        string language,
        CourseLevel level,
        ICollection<Category> categories, 
        int numberOfStudents, 
        int numberOfModules, 
        int numberOfLessons,
        int numberOfAssignments,
        int numberOfHours
        )
    {
        CourseId = courseId;
        CourseTitle = courseTitle;
        CourseDescription = courseDescription;
        ThumbnailUrl = thumbnailUrl;
        Language = language;
        Level = level;
        Categories = categories;
        NumberOfStudents = numberOfStudents;
        NumberOfModules = numberOfModules;
        NumberOfLessons = numberOfLessons;
        NumberOfAssignments = numberOfAssignments;
        NumberOfMinutes = numberOfHours;
    }
}
