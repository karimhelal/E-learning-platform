using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Core.RepositoryInterfaces;
using Core.Entities;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

using Web.ViewModels;
using Web.ViewModels.Student;
using static System.Net.WebRequestMethods;
using BLL.Interfaces.Student;
using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment

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
    private readonly Web.Interfaces.IStudentCertificatesService _certificatesService;
    private readonly IWebHostEnvironment _webHostEnvironment; // Inject IWebHostEnvironment

    public StudentController(
        Web.Interfaces.IStudentDashboardService dashboardService,
        Web.Interfaces.IStudentCoursesService coursesService,
        Web.Interfaces.IStudentTrackService tracksService,
        Web.Interfaces.IStudentTrackDetailsService trackDetailsService,
        Web.Interfaces.ICurrentUserService currentUserService,
        Web.Interfaces.IStudentBrowseTrackService browseTrackService,
        Web.Interfaces.IStudentBrowseCoursesService browseCoursesService,
        IHttpContextAccessor http,
        Web.Interfaces.IStudentCourseDetailsService courseDetailsService,
        IUserRepository userRepository,
        IStudentProfileService studentProfileService,
        Web.Interfaces.IStudentCertificatesService certificatesService,
        AppDbContext context,
        IWebHostEnvironment webHostEnvironment) // Receive IWebHostEnvironment
    {
        _dashboardService = dashboardService;
        _coursesService = coursesService;
        _tracksService = tracksService;
        _trackDetailsService = trackDetailsService;
        _currentUserService = currentUserService;
        _browseTrackService = browseTrackService;
        _browseCoursesService = browseCoursesService;
        _http = http;
        _courseDetailsService = courseDetailsService;
        _userRepository = userRepository;
        _studentProfileService = studentProfileService;
        _certificatesService = certificatesService;
        _context = context;
        _webHostEnvironment = webHostEnvironment; // Assign to field
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
        {
            Console.WriteLine("GetStudentIdAsync: userId is 0 - user not authenticated");
            return 0;
        }

        Console.WriteLine($"GetStudentIdAsync: Looking for student profile for userId: {userId}");

        var studentProfile = await _userRepository.GetStudentProfileForUserAsync(userId, includeUserBase: false);

        // If no profile exists, create one automatically
        if (studentProfile == null)
        {
            Console.WriteLine($"GetStudentIdAsync: No student profile found, creating one for userId: {userId}");
            
            try
            {
                studentProfile = new StudentProfile
                {
                    UserId = userId,
                    Bio = "New learner on the platform"
                };

                _context.StudentProfiles.Add(studentProfile);
                await _context.SaveChangesAsync();

                Console.WriteLine($"GetStudentIdAsync: Created student profile with ID: {studentProfile.StudentId}");

                // Reload the profile to get the auto-generated StudentId
                studentProfile = await _userRepository.GetStudentProfileForUserAsync(userId, includeUserBase: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetStudentIdAsync: Error creating student profile: {ex.Message}");
                Console.WriteLine($"GetStudentIdAsync: Inner exception: {ex.InnerException?.Message}");
                return 0;
            }
        }

        var result = studentProfile?.StudentId ?? 0;
        Console.WriteLine($"GetStudentIdAsync: Returning studentId: {result}");
        return result;
    }

    [HttpGet("/student/dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Title = "Student Dashboard | Masar";

        try
        {
            var studentId = await GetStudentIdAsync();

            if (studentId == 0)
            {
                // Log more details about why this failed
                var userId = _currentUserService.GetUserId();
                Console.WriteLine($"Dashboard: Failed to get student profile. UserId: {userId}");
                
                // Redirect to a more helpful error page or try to create profile
                return Content($"Student profile not found. Please contact support. (User ID: {userId})");
            }

            Console.WriteLine($"Dashboard: Getting dashboard data for studentId: {studentId}");
            
            Web.Interfaces.StudentDashboardData? dashboardData = null;
            
            try
            {
                dashboardData = await _dashboardService.GetDashboardDataAsync(studentId);
            }
            catch (Exception serviceEx)
            {
                Console.WriteLine($"Dashboard: Service error - {serviceEx.Message}");
                Console.WriteLine($"Dashboard: Inner exception - {serviceEx.InnerException?.Message}");
            }

            if (dashboardData == null)
            {
                Console.WriteLine($"Dashboard: dashboardData is null for studentId: {studentId}");
                
                // Try to return a minimal dashboard instead of an error
                var studentProfile = await _userRepository.GetStudentProfileForUserAsync(
                    _currentUserService.GetUserId(), 
                    includeUserBase: true);
                
                if (studentProfile?.User != null)
                {
                    // Return minimal dashboard data
                    dashboardData = new Web.Interfaces.StudentDashboardData
                    {
                        StudentId = studentProfile.StudentId,
                        StudentName = studentProfile.User.FirstName,
                        UserInitials = $"{studentProfile.User.FirstName[0]}{studentProfile.User.LastName[0]}".ToUpper(),
                        Stats = new Web.Interfaces.DashboardStats(),
                        ContinueLearningCourses = new List<Web.Interfaces.ContinueLearningCourse>(),
                        EnrolledTracks = new List<Web.Interfaces.EnrolledTrack>()
                    };
                }
                else
                {
                    return NotFound("Dashboard data could not be loaded. Please try again later.");
                }
            }

            var viewModel = new StudentDashboardViewModel
            {
                Data = dashboardData,
                PageTitle = "Student Dashboard",
                GreetingMessage = GetGreeting()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dashboard Error: {ex.Message}");
            Console.WriteLine($"Dashboard Inner: {ex.InnerException?.Message}");
            Console.WriteLine($"Dashboard Stack Trace: {ex.StackTrace}");
            return Content($"An error occurred: {ex.Message}\n\nInner: {ex.InnerException?.Message}");
        }
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

        var studentId = await GetStudentIdAsync();

        if (studentId == 0)
            return Unauthorized("Student profile not found");

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

    [HttpGet("/student/course/details/{courseId}")]
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
                UserInitials = initials,
                Username = $"@{profileData.FirstName.ToLower()}_{profileData.LastName.ToLower()}",
                Email = profileData.Email,
                Phone = profileData.Phone,
                ProfilePicture = profileData.ProfilePicture,
                Bio = profileData.Bio,
                Initials = initials,
                Location = profileData.Location,
                JoinedDate = profileData.JoinedDate.ToString("MMMM yyyy"),
                GithubUrl = profileData.GithubUrl,
                LinkedInUrl = profileData.LinkedInUrl,
                FacebookUrl = profileData.FacebookUrl,
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
                Skills = profileData.Skills,
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

    /// <summary>
    /// Get Edit Profile Form - Returns the edit profile partial view for the modal.
    /// </summary>
    [HttpGet("/student/profile/edit")]
    public async Task<IActionResult> GetEditProfileForm()
    {
        var studentId = await GetStudentIdAsync();
        if (studentId == 0)
            return Unauthorized("Student profile not found");

        var profileData = await _studentProfileService.GetStudentProfileAsync(studentId);

        if (profileData == null)
            return NotFound();

        var formModel = new EditStudentProfileFormModel
        {
            FirstName = profileData.FirstName,
            LastName = profileData.LastName,
            Phone = profileData.Phone,
            Location = profileData.Location,
            Bio = profileData.Bio,
            GithubUrl = profileData.GithubUrl,
            LinkedInUrl = profileData.LinkedInUrl,
            FacebookUrl = profileData.FacebookUrl,
            WebsiteUrl = profileData.WebsiteUrl,
            
            // Skills from database
            Skills = profileData.Skills ?? new List<string>()
        };

        return PartialView("_EditProfileModal", formModel);
    }

    /// <summary>
    /// Upload Image - Handle profile photo and cover image uploads.
    /// </summary>
    [HttpPost("/student/upload-image")]
    [IgnoreAntiforgeryToken] // Allow upload without token since we handle it manually
    public async Task<IActionResult> UploadImage(IFormFile file, string type)
    {
        Console.WriteLine($"UploadImage called - file: {file?.FileName}, type: {type}");
        
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return Json(new { success = false, message = "Invalid file type. Allowed: jpg, jpeg, png, webp, gif" });

        if (file.Length > 5 * 1024 * 1024)
            return Json(new { success = false, message = "File size exceeds 5MB" });

        try
        {
            var studentId = await GetStudentIdAsync();
            Console.WriteLine($"UploadImage - studentId: {studentId}");
            
            if (studentId == 0)
                return Json(new { success = false, message = "Student profile not found" });

            var folder = type == "cover" ? "covers" : "profiles";
            var fileName = $"{folder}-student-{studentId}-{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folder);
            
            Console.WriteLine($"UploadImage - uploadsFolder: {uploadsFolder}");
            
            Directory.CreateDirectory(uploadsFolder);
            
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            var fileUrl = $"/uploads/{folder}/{fileName}";
            Console.WriteLine($"UploadImage - fileUrl: {fileUrl}");

            // If it's a profile picture, update the user's picture in database
            if (type == "profile")
            {
                var studentProfile = await _userRepository.GetStudentProfileAsync(studentId, includeUserBase: true);
                if (studentProfile?.User != null)
                {
                    studentProfile.User.Picture = fileUrl;
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"UploadImage - saved picture to database");
                }
            }

            return Json(new { success = true, url = fileUrl });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UploadImage error: {ex.Message}");
            Console.WriteLine($"UploadImage stack: {ex.StackTrace}");
            return Json(new { success = false, message = "Upload failed: " + ex.Message });
        }
    }

    /// <summary>
    /// Update Profile - Handle profile updates from the edit form.
    /// </summary>
    [HttpPost("/student/profile/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateStudentProfileRequest request)
    {
        var studentId = await GetStudentIdAsync();
        if (studentId == 0)
            return Json(new { success = false, message = "Student profile not found" });

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, message = "Validation failed", errors });
        }

        var updateDto = new BLL.DTOs.Student.UpdateStudentProfileDto
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Location = request.Location,
            Bio = request.Bio,
            Skills = request.Skills ?? new List<string>(),
            GithubUrl = request.GithubUrl,
            LinkedInUrl = request.LinkedInUrl,
            FacebookUrl = request.FacebookUrl,
            WebsiteUrl = request.WebsiteUrl
        };

        var result = await _studentProfileService.UpdateStudentProfileAsync(studentId, updateDto);

        if (result)
            return Json(new { success = true, message = "Profile updated successfully!" });
        else
            return Json(new { success = false, message = "Failed to update profile" });
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

    public class UpdateStudentProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? Bio { get; set; }
        public List<string>? Skills { get; set; }
        public string? GithubUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string? WebsiteUrl { get; set; }
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

    [HttpGet("/student/certificates")]
    public async Task<IActionResult> Certificates()
    {
        ViewBag.Title = "My Certificates | Masar";

        var studentId = await GetStudentIdAsync();

        if (studentId == 0)
            return Unauthorized("Student profile not found");

        var certificatesData = await _certificatesService.GetStudentCertificatesAsync(studentId);

        if (certificatesData == null)
        {
            return NotFound("Student profile not found");
        }

        var viewModel = new StudentCertificatesViewModel
        {
            Data = certificatesData,
            PageTitle = "My Certificates"
        };

        return View(viewModel);
    }
}
