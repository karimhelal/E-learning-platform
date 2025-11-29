using Microsoft.AspNetCore.Mvc;
using Web.Interfaces;
using Web.ViewModels.Student;

namespace Web.Controllers.Student;

public class StudentController : Controller
{
    private readonly IStudentDashboardService _dashboardService;
    private readonly IStudentCoursesService _coursesService;
    private readonly IStudentCourseDetailsService _courseDetailsService;

    public StudentController(
        IStudentDashboardService dashboardService,
        IStudentCoursesService coursesService,
        IStudentCourseDetailsService courseDetailsService)
    {
        _dashboardService = dashboardService;
        _coursesService = coursesService;
        _courseDetailsService = courseDetailsService;
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

    [HttpGet("/student/course/{courseId}/details")]
    public async Task<IActionResult> CourseDetails(int courseId)
    {
        ViewBag.Title = "Course Details | Masar";

        var studentId = 1001; // TODO: Get from authenticated user
        var courseData = await _courseDetailsService.GetCourseDetailsAsync(studentId, courseId);

        if (courseData == null)
        {
            return NotFound();
        }

        var viewModel = new StudentCourseDetailsViewModel
        {
            Data = courseData,
            PageTitle = courseData.Title
        };

        return View(viewModel);
    }

    [HttpPost("/student/lesson/{lessonId}/toggle")]
    public async Task<IActionResult> ToggleLessonCompletion(int lessonId, [FromBody] ToggleRequest request)
    {
        var studentId = 1001; // TODO: Get from authenticated user
        
        var result = await _courseDetailsService.ToggleLessonCompletionAsync(
            studentId, 
            lessonId, 
            request.IsCompleted
        );

        return Json(new { success = result });
    }

    private string GetGreeting()
    {
        var hour = DateTime.Now.Hour;
        return hour < 12 ? "Good Morning" : hour < 18 ? "Good Afternoon" : "Good Evening";
    }

    public class ToggleRequest
    {
        public bool IsCompleted { get; set; }
    }
}