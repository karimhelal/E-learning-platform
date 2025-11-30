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
    private readonly IStudentCourseDetailsService _courseDetailsService;

    private readonly int userId = 35;

    public StudentController(
        IStudentDashboardService dashboardService,
        IStudentCoursesService coursesService,
        IStudentTrackService tracksService,
        IStudentTrackDetailsService trackDetailsService,
        ICurrentUserService currentUserService,
        IStudentCourseDetailsService courseDetailsService)  // ✅ ADD THIS PARAMETER
    {
        _dashboardService = dashboardService;
        _coursesService = coursesService;
        _tracksService = tracksService;
        _trackDetailsService = trackDetailsService;
        _currentUserService = currentUserService;
        _courseDetailsService = courseDetailsService;  // ✅ ADD THIS LINE
    }

    [HttpGet("/student/dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Title = "Student Dashboard | Masar";

        var userId = 35; // TODO : Replace with actual logged-in student ID retrieval
       // var userId = _currentUserService.GetUserId();
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

    [HttpGet("/student/course/{courseId}/details")]
    public async Task<IActionResult> CourseDetails(int courseId)
    {
        ViewBag.Title = "Course Details | Masar";

        var courseData = await _courseDetailsService.GetCourseDetailsAsync(userId, courseId);

        if (courseData == null)
        {
            return NotFound("Course not found or not enrolled");
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
        var result = await _courseDetailsService.ToggleLessonCompletionAsync(
            userId,
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