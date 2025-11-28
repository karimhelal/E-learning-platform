using Web.Interfaces;

namespace Web.ViewModels.Student;

public class StudentDashboardViewModel
{
    public StudentDashboardData Data { get; set; } = new();
    public string PageTitle { get; set; } = "Student Dashboard";
    public string GreetingMessage { get; set; } = "Welcome back";
}