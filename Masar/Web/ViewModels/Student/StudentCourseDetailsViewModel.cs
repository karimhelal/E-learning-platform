using BLL.DTOs.Student;

namespace Web.ViewModels.Student;

public class StudentCourseDetailsViewModel
{
    public StudentCourseDetailsDto Data { get; set; } = new();
    public string PageTitle { get; set; } = "Course Details";
}