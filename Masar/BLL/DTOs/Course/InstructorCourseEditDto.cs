using Core.Entities;
using Core.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Course;

public class InstructorCourseEditDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ThumbnailImageUrl { get; set; }
    public ICollection<Category> Categories { get; set; }
    public CourseLevel Level { get; set; }


    public int NumberOfStudents { get; set; }
    public ICollection<CourseLearningOutcome> LearningOutcomes { get; set; }
}
