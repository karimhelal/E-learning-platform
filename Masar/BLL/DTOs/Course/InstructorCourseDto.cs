using Core.Entities;
using Core.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Course;

public class InstructorCourseDto
{
    public int CourseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ThumbnailImageUrl { get; set; }
    public string Status { get; set; }
    public DateOnly CreatedDate { get; set; }
    public DateOnly LastUpdatedDate { get; set; }
    public CourseLevel Level { get; set; }
    public Category MainCategory { get; set; }


    public int NumberOfStudents { get; set; }
    public int NumberOfModules { get; set; }
    public int NumberOfMinutes { get; set; }
    public int NumberOfHours => Math.Max(1, NumberOfMinutes / 60);
    public float AverageRating { get; set; }
}
