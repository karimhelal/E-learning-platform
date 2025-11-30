using Web.Interfaces;

namespace Web.ViewModels.Student;

public class StudentCourseDetailsViewModel
{
    public StudentCourseDetailsData Data { get; set; } = new();
    public string PageTitle { get; set; } = "Course Details";
}