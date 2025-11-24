using Core.Entities;
using Core.Entities.Enums;

namespace BLL.DTOs.Course;

public class InstructorCourseBasicDetailsDto
{
    public int CourseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ThumbnailImageUrl { get; set; }
    public Category MainCategory { get; set; }
    public CourseLevel Level { get; set; }
    public ICollection<Category> AdiitionalCategories { get; set; }
    public ICollection<CourseLearningOutcome> LearningOutcomes { get; set; }


    public int InstructorId { get; set; }
}
