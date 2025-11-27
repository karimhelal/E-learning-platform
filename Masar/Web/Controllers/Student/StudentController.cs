using Microsoft.AspNetCore.Mvc;
using Web.Interfaces;
using Web.ViewModels.Student;

namespace Web.Controllers.Student;

public class StudentController : Controller
{
    private readonly IStudentDashboardService _dashboardService;
    private readonly IStudentCoursesService _coursesService;

    public StudentController(IStudentDashboardService dashboardService, IStudentCoursesService coursesService)
    {
        _dashboardService = dashboardService;
        _coursesService = coursesService;

    }

    [HttpGet("/student/dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Title = "Student Dashboard | Masar";

        var studentId = 1001; // TODO : Replace with actual logged-in student ID retrieval
        var dashboardData = await _dashboardService.GetDashboardDataAsync(studentId);

        if (dashboardData == null)
        {
            return NotFound("Student profile not found");
        }

        var viewModel = new StudentDashboardViewModel
        {
            Data = dashboardData,
            PageTitle = "Student Dashboard",
            GreetingMessage = GetGreeting()
        };

        return View(viewModel);
    }


    [HttpGet("/student/my-courses")]
    public async Task<IActionResult> MyCourses()
    {
        ViewBag.Title = "My Courses | Masar";

        var studentId = 1001; // TODO: Replace with actual logged-in student ID retrieval
        var coursesData = await _coursesService.GetMyCoursesAsync(studentId);

        if (coursesData == null)
        {
            return NotFound("Student profile not found");
        }

        var viewModel = new StudentCoursesViewModel
        {
            Data = coursesData,
            PageTitle = "My Courses"
        };

        return View(viewModel);
    }

    private string GetGreeting()
    {
        var hour = DateTime.Now.Hour;
        return hour < 12 ? "Good Morning" : hour < 18 ? "Good Afternoon" : "Good Evening";
    }
}