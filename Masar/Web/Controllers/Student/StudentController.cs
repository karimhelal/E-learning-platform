using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Core.RepositoryInterfaces;
using Core.Entities;
using DAL.Data;

using Web.ViewModels;
using Web.ViewModels.Student;
using static System.Net.WebRequestMethods;
using BLL.Interfaces.Student;  // Only for IStudentProfileService

namespace Web.Controllers.Student;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly Web.Interfaces.IStudentDashboardService _dashboardService;
    private readonly Web.Interfaces.IStudentCoursesService _coursesService;
    private readonly Web.Interfaces.IStudentTrackService _tracksService;
    private readonly Web.Interfaces.IStudentTrackDetailsService _trackDetailsService;
    private readonly Web.Interfaces.ICurrentUserService _currentUserService;
    private readonly Web.Interfaces.IStudentBrowseTrackService _browseTrackService;
    private readonly Web.Interfaces.IStudentBrowseCoursesService _browseCoursesService;
    private readonly IHttpContextAccessor _http;
    private readonly Web.Interfaces.IStudentCourseDetailsService _courseDetailsService;
    private readonly IUserRepository _userRepository;
    private readonly AppDbContext _context;
    private readonly IStudentProfileService _studentProfileService;

    public StudentController(
        Web.Interfaces.IStudentDashboardService dashboardService,
        Web.Interfaces.IStudentCoursesService coursesService,
        Web.Interfaces.IStudentTrackService tracksService,
        Web.Interfaces.IStudentTrackDetailsService trackDetailsService,
        Web.Interfaces.ICurrentUserService currentUserService,
        Web.Interfaces.IStudentBrowseTrackService browseTrackService,
        Web.Interfaces.IStudentBrowseCoursesService browseCoursesService, // ADD THIS
        IHttpContextAccessor http,
        Web.Interfaces.IStudentCourseDetailsService courseDetailsService,
        IUserRepository userRepository,
        IStudentProfileService studentProfileService,
        AppDbContext context)
    {
        _dashboardService = dashboardService;
        _coursesService = coursesService;
        _tracksService = tracksService;
        _trackDetailsService = trackDetailsService;
        _currentUserService = currentUserService;
        _browseTrackService = browseTrackService;
        _browseCoursesService = browseCoursesService; // ADD THIS
        _http = http;
        _courseDetailsService = courseDetailsService;
        _userRepository = userRepository;
        _studentProfileService = studentProfileService;
        _context = context;
    }

    /// <summary>
    /// Helper method to get student ID from logged-in user.
    /// Returns the StudentProfile ID (not User ID) of the authenticated user.
    /// Automatically creates a StudentProfile if one doesn't exist.
    /// </summary>
    private async Task<int> GetStudentIdAsync()
    {
        var userId = _currentUserService.GetUserId();
        
        if (userId == 0)
            return 0;
        
        var studentProfile = await _userRepository.GetStudentProfileForUserAsync(userId, includeUserBase: false);
        
        // If no profile exists, create one automatically
        if (studentProfile == null)
        {
            studentProfile = new StudentProfile
            {
                UserId = userId,
                Bio = "New learner on the platform"
            };

            _context.StudentProfiles.Add(studentProfile);
            await _context.SaveChangesAsync();

            // Reload the profile to get the auto-generated StudentId
            studentProfile = await _userRepository.GetStudentProfileForUserAsync(userId, includeUserBase: false);
        }
        
        return studentProfile?.StudentId ?? 0;
    }

    [HttpGet("/student/dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Title = "Student Dashboard | Masar";

        //var userId = 2; // TODO : Replace with actual logged-in student ID retrieval
        var userId = _currentUserService.GetUserId();
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

    [HttpGet("/student/my-tracks")]
    public async Task<IActionResult> MyTracks()
    {
        ViewBag.Title = "My Tracks | Masar";

        var studentId = await GetStudentIdAsync();
        
        if (studentId == 0)
            return Unauthorized("Student profile not found");

        var tracksData = await _tracksService.GetStudentTracksAsync(studentId);

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

        var studentId = await GetStudentIdAsync();
        
        if (studentId == 0)
            return Unauthorized("Student profile not found");

        var data = await _trackDetailsService.GetTrackDetailsAsync(studentId, trackId);
        if (data == null)
            return NotFound("Track not found or not enrolled");

        var vm = new StudentTrackDetailsViewModel
        {
            Data = data,
            PageTitle = data.Title
        };

        return View(vm);
    }

    [HttpGet("/student/browse-tracks")]
    public async Task<IActionResult> BrowseTracks()
    {
        var studentId = await GetStudentIdAsync();
        
        if (studentId == 0)
            return Unauthorized("Student profile not found");

        var data = await _browseTrackService.GetAllTracksAsync(studentId);
        if (data == null)
            return View("Error");

        var vm = new StudentBrowseTracksViewModel { Data = data };

        return View(vm);
    }

    [HttpGet("/student/my-courses")]
    public async Task<IActionResult> MyCourses()
    {
        ViewBag.Title = "My Courses | Masar";

       // var studentId = 1001; // TODO: Replace with actual logged-in student ID retrieval
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

    [HttpGet("/student/course/{courseId}/details")]
    public async Task<IActionResult> CourseDetails(int courseId)
    {
        ViewBag.Title = "Course Details | Masar";

        var studentId = await GetStudentIdAsync();
        
        if (studentId == 0)
            return Unauthorized("Student profile not found");

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
        var studentId = await GetStudentIdAsync();
        
        if (studentId == 0)
            return Unauthorized();

        var result = await _courseDetailsService.ToggleLessonCompletionAsync(
            studentId,
            lessonId,
            request.IsCompleted
        );

        return Json(new { success = result });
    }

    [HttpGet("/student/profile")]
    public async Task<IActionResult> Profile()
    {
        ViewBag.Title = "My Profile | Masar";

        var studentId = await GetStudentIdAsync();
        
        if (studentId == 0)
            return Unauthorized("Student profile not found");

        var profileData = await _studentProfileService.GetStudentProfileAsync(studentId);

        if (profileData == null)
        {
            return NotFound("Profile not found");
        }

        var initials = $"{profileData.FirstName[0]}{profileData.LastName[0]}".ToUpper();

        var viewModel = new StudentProfileViewModel
        {
            Data = new StudentProfileDataViewModel
            {
                StudentId = profileData.StudentId,
                UserId = profileData.UserId,
                FirstName = profileData.FirstName,
                LastName = profileData.LastName,
                FullName = profileData.FullName,
                StudentName = profileData.FullName,
                UserInitials = initials, // Added this line
                Username = $"@{profileData.FirstName.ToLower()}_{profileData.LastName.ToLower()}",
                Email = profileData.Email,
                Phone = profileData.Phone,
                ProfilePicture = profileData.ProfilePicture,
                Bio = profileData.Bio,
                Initials = initials,
                Location = profileData.Location,
                Languages = profileData.Languages,
                JoinedDate = profileData.JoinedDate.ToString("MMMM yyyy"),
                GithubUrl = profileData.GithubUrl,
                LinkedInUrl = profileData.LinkedInUrl,
                TwitterUrl = profileData.TwitterUrl,
                WebsiteUrl = profileData.WebsiteUrl,
                Stats = new StudentProfileStatsViewModel
                {
                    TotalCoursesEnrolled = profileData.TotalCoursesEnrolled,
                    CompletedCourses = profileData.CompletedCourses,
                    ActiveCourses = profileData.ActiveCourses,
                    CertificatesEarned = profileData.CertificatesEarned,
                    TotalLearningHours = profileData.TotalLearningHours,
                    CurrentStreak = profileData.CurrentStreak
                },
                LearningInterests = profileData.LearningInterests,
                EnrolledCourses = profileData.EnrolledCourses.Select(c => new StudentProfileCourseCardViewModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    ThumbnailImageUrl = c.ThumbnailImageUrl,
                    ProgressPercentage = c.ProgressPercentage,
                    Status = c.Status,
                    InstructorName = c.InstructorName
                }).ToList(),
                Certificates = profileData.Certificates.Select(cert => new StudentCertificateViewModel
                {
                    CertificateId = cert.CertificateId,
                    Title = cert.Title,
                    IssuedDate = cert.IssuedDate.ToString("MMM dd, yyyy"),
                    Type = cert.Type
                }).ToList()
            },
            PageTitle = "My Profile"
        };

        return View(viewModel);
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

    [HttpGet("/student/browse-courses")]
    public async Task<IActionResult> BrowseCourses()
    {
        ViewBag.Title = "Browse Courses | Masar";

        var studentId = await GetStudentIdAsync();
        
        if (studentId == 0)
            return Unauthorized("Student profile not found");

        var data = await _browseCoursesService.GetAllCoursesAsync(studentId);
        if (data == null)
            return View("Error");

        var vm = new StudentBrowseCoursesViewModel { Data = data };

        return View(vm);
    }
}

































































































































































































































































































































































































































































































































































































































































































































































