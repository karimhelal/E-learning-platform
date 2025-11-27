using Web.Interfaces;

namespace Web.ViewModels.Student;

public class StudentCoursesViewModel
{
    public StudentCoursesData Data { get; set; } = new();
    public string PageTitle { get; set; } = "My Courses";
}