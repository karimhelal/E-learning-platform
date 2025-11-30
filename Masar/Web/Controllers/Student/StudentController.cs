using Microsoft.AspNetCore.Mvc;
using Web.Interfaces;
using Web.ViewModels.Student;
using Web.ViewModels;

namespace Web.Controllers.Student;

public class StudentController : Controller
{
    private readonly IStudentDashboardService _dashboardService;
    private readonly IStudentCoursesService _coursesService;
    private readonly IStudentTrackService _tracksService;
    private readonly IStudentTrackDetailsService _trackDetailsService;
    private readonly ICurrentUserService _currentUserService;
    private readonly int userId = 1001;

    public StudentController(
        IStudentDashboardService dashboardService,
        IStudentCoursesService coursesService,
        IStudentTrackService tracksService,
        IStudentTrackDetailsService trackDetailsService,
        ICurrentUserService currentUserService)
    {
        _dashboardService = dashboardService;
        _coursesService = coursesService;
        _tracksService = tracksService;
        _trackDetailsService = trackDetailsService;
        _currentUserService = currentUserService;
    }

    [HttpGet("/student/dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Title = "Student Dashboard | Masar";

        //var userId = 2; // TODO : Replace with actual logged-in student ID retrieval
        var dashboardData = await _dashboardService.GetDashboardDataAsync(userId);

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

        //var userId = 2; // TODO: Replace with actual logged-in student ID retrieval
        var coursesData = await _coursesService.GetMyCoursesAsync(userId);

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

    [HttpGet("/student/my-tracks")]
    public async Task<IActionResult> MyTracks()
    {
        ViewBag.Title = "My Tracks | Masar";

        //var userId = 2; // TODO: Replace with actual logged-in student ID
        var tracksData = await _tracksService.GetStudentTracksAsync(userId);

        if (tracksData == null)
        {
            return NotFound("Student tracks not found");
        }

        var viewModel = new StudentTracksViewModel
        {
            PageTitle = "My Tracks",
            Data = new Web.ViewModels.StudentTracksData
            {
                StudentId = tracksData.StudentId,
                StudentName = tracksData.StudentName,
                UserInitials = tracksData.UserInitials,
                Tracks = tracksData.Tracks
                    .Select(t => new EnrolledTrackViewModel
                    {
                        TrackId = t.TrackId,
                        Title = t.Title,
                        Description = t.Description,
                        CoursesCount = t.CoursesCount,
                        Progress = (int)t.ProgressPercentage,
                        Status = t.Status,
                        IconClass = t.IconClass,
                        ActionText = t.ActionText,
                        ActionUrl = t.ActionUrl,
                        Courses = t.Courses
                            .Select(c => new CoursePreviewViewModel
                            {
                                Id = c.CourseId,
                                Title = c.Title,
                                Status = c.Status,
                                Icon = c.IconClass
                            })
                            .ToList()
                    })
                    .ToList()
            }
        };

        return View(viewModel);
    }
    [HttpGet("/student/track/{trackId}")]
    public async Task<IActionResult> TrackDetails(int trackId)
    {
        ViewBag.Title = "Track Details | Masar";

        //int userId = 2; // TODO: replace with actual logged-in student ID
        var data = await _trackDetailsService.GetTrackDetailsAsync(userId, trackId);
        if (data == null)
            return NotFound("Track not found or not enrolled");

        var vm = new StudentTrackDetailsViewModel
        {
            Data = data,
            PageTitle = data.Title
        };

        return View(vm);
    }

    private string GetGreeting()
    {
        var hour = DateTime.Now.Hour;
        return hour < 12 ? "Good Morning" : hour < 18 ? "Good Afternoon" : "Good Evening";
    }
}