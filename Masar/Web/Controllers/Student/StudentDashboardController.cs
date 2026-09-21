using BLL.Interfaces.Student;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.AspNetCore.Mvc;
using Web.Interfaces;
using Web.ViewModels.Student;

namespace Web.Controllers.Student;

public class StudentDashboardController(
    IStudentDashboardService dashboardService,
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    AppDbContext context,
    ILogger<StudentDashboardController> logger)
    : StudentBaseController(currentUserService, userRepository, context, logger)
{
    [HttpGet("/student/dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Title = "Student Dashboard | Masar";
        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Content("Student profile not found. Please contact support.");

        var dashboardData = await dashboardService.GetDashboardDataAsync(studentId);
        if (dashboardData == null) return NotFound("Dashboard data could not be loaded.");

        return View(new StudentDashboardViewModel
        {
            Data = dashboardData,
            PageTitle = "Student Dashboard",
            GreetingMessage = GetGreeting()
        });
    }

    private string GetGreeting()
    {
        var hour = DateTime.Now.Hour;
        return hour < 12 ? "Good Morning" : hour < 18 ? "Good Afternoon" : "Good Evening";
    }
}