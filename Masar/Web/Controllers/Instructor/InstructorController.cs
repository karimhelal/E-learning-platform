using BLL.DTOs.Misc;
using BLL.Helpers;
using BLL.Interfaces.Instructor;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Instructor;
using Web.ViewModels.Instructor.Dashboard;

namespace Web.Controllers.Instructor;

public class InstructorController : Controller
{
    private readonly IInstructorDashboardService _dashboardService;
    private readonly IInstructorCoursesService _coursesService;
    private readonly IInstructorProfileService _profileService;
    private readonly RazorViewToStringRenderer _razorRenderer;

    public InstructorController(
        IInstructorDashboardService dashboardService, 
        IInstructorCoursesService courseService,
        IInstructorProfileService profileService,
        RazorViewToStringRenderer razorRenderer)
    {
        _dashboardService = dashboardService;
        _coursesService = courseService;
        _profileService = profileService;
        _razorRenderer = razorRenderer;
    }


    [HttpGet("/instructor/dashboard")]
    [HttpGet("/")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Title = "Instructor Dashboard | Masar";

        var instructorId = 3;     // TODO: Get from logged in user context
        var dashboardData = await _dashboardService.GetInstructorDashboardAsync(instructorId);

        var viewModel = new InstructorDashboardViewModel
        {
            Data = new InstructorDashboardDataViewModel
            {
                InstructorId = dashboardData.InstructorId,
                InstructorName = dashboardData.InstructorName,

                GeneralStats = new InstructorGeneralStatsViewModel
                {
                    TotalCourses = dashboardData.GeneralStats.TotalCourses,
                    TotalStudents = dashboardData.GeneralStats.TotalStudents,
                    CompletionRate = dashboardData.GeneralStats.CompletionRate,
                    AverageRating = dashboardData.GeneralStats.AverageRating
                },

                CurrentMonthStats = new InstructorCurrentMonthStatsViewModel
                {
                    NewStudents = dashboardData.CurrentMonthStats.NewStudents,
                    NewReviews = dashboardData.CurrentMonthStats.NewReviews,
                    Completions = dashboardData.CurrentMonthStats.Completions
                },

                TopPerformingCourses = dashboardData.TopPerformingCourses.Select(c => 
                    new InstructorTopPerformingCourseViewModel
                    {
                        Title = c.Title,
                        StudentsEnrolled = c.StudentsEnrolled,
                        AverageRating = c.AverageRating
                    }
                ),

                CourseCards = dashboardData.CourseCards.Select(c => 
                    new CourseCardViewModel
                    {
                        CourseId = c.CourseId,
                        Title = c.Title,
                        Description = c.Description,
                        
                        MainCategory = c.MainCategory?.Name ??  "Undefined",
                        Level = c.Level.ToString(),

                        ModulesCount = c.ModulesCount,
                        StudentsCount = c.StudentsCount,
                        DurationHours = c.DurationHours,
                        
                        Rating = c.Rating,
                        Status = c.Status
                    }
                )
            },


            // UI specific properties
            PageTitle = "Instructor Overview",
            // Dynamic greeting message based on time of day
            GreetingMessage = DateTime.Now.Hour < 12 ? "Good Morning" : "Good Evening",
        };

        return View(viewModel);
    }


    [HttpGet("/instructor/my-courses")]
    public async Task<IActionResult> MyCourses()
    {
        ViewBag.Title = "My Courses | Masar";
        
        var userId = 1;     // TODO: Get from logged in user context
        var initialRequest = new PagingRequest() { CurrentPage = 1, PageSize = 4 }; // Default paging request
        var coursesData = await _coursesService.GetInstructorCoursesPagedAsync(userId, initialRequest);
        
        var viewModel = new InstructorCoursesViewModel
        {
            Data = new PagedResultViewModel<InstructorCourseViewModel>
            {
                Settings = new PaginationSettingsViewModel
                {
                    CurrentPage = coursesData.CurrentPage,
                    PageSize = coursesData.PageSize,
                    TotalPages = coursesData.TotalPages
                },

                Items = coursesData.Items.Select(c => new InstructorCourseViewModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    ThumbnailImageUrl = c.ThumbnailImageUrl,
                    Status = c.Status,

                    CreatedDate = c.CreatedDate.ToString("MM-dd-yyyy"),
                    LastUpdatedDate = c.LastUpdatedDate.ToString("MM-dd-yyyy"),

                    Level = c.Level.ToString(),
                    MainCategory = c.MainCategory.Name ?? "Undefined",

                    NumberOfStudents = c.NumberOfStudents,
                    NumberOfModules = c.NumberOfModules,
                    NumberOfMinutes = c.NumberOfMinutes,
                    AverageRating = c.AverageRating
                })
            },

            // UI specific properties
            PageTitle = "My Courses | Masar",
        };

        return View(viewModel);
    }

    [HttpPost("/instructor/my-courses")]
    public async Task<IActionResult> GetCoursesPartial([FromBody] PagingRequest request)
    {
        var userId = 1;     // TODO: Get from logged in user context
        var coursesData = await _coursesService.GetInstructorCoursesPagedAsync(userId, request);
        
        var pagedResult = new PagedResultViewModel<InstructorCourseViewModel>
        {
            Settings = new PaginationSettingsViewModel
            {
                CurrentPage = coursesData.CurrentPage,
                PageSize = coursesData.PageSize,
                TotalPages = coursesData.TotalPages
            },

            Items = coursesData.Items.Select(c => new InstructorCourseViewModel
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                ThumbnailImageUrl = c.ThumbnailImageUrl,
                Status = c.Status,

                CreatedDate = c.CreatedDate.ToString("MM-dd-yyyy"),
                LastUpdatedDate = c.LastUpdatedDate.ToString("MM-dd-yyyy"),

                Level = c.Level.ToString(),
                MainCategory = c.MainCategory.Name ?? "Undefined",

                NumberOfStudents = c.NumberOfStudents,
                NumberOfModules = c.NumberOfModules,
                NumberOfMinutes = c.NumberOfMinutes,
                AverageRating = c.AverageRating
            })
        };

        var CoursesGrid = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_CoursesGridPartialView", pagedResult.Items);
        var Pagination = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_PaginationPartialView", pagedResult.Settings);

        return Json(new
        {
            CoursesGrid = CoursesGrid,
            Pagination = Pagination
        });
    }

    [HttpGet("/instructor/profile")]
    public async Task<IActionResult> Profile()
    {
        ViewBag.Title = "Instructor Profile | Masar";

        var instructorId = 3; // TODO: Get from logged in user context
        var profileData = await _profileService.GetInstructorProfileAsync(instructorId);

        if (profileData == null)
        {
            return NotFound();
        }

        // Generate random gradients for courses
        var gradients = new[]
        {
            "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
            "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
            "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)",
            "linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)",
            "linear-gradient(135deg, #fa709a 0%, #fee140 100%)",
            "linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)"
        };

        var viewModel = new InstructorProfileViewModel
        {
            Data = new InstructorProfileDataViewModel
            {
                InstructorId = profileData.InstructorId,
                UserId = profileData.UserId,
                FirstName = profileData.FirstName,
                LastName = profileData.LastName,
                FullName = profileData.FullName,
                Username = $"@{profileData.FirstName.ToLower()}_{profileData.LastName.ToLower()}",
                Email = profileData.Email,
                Phone = profileData.Phone,
                ProfilePicture = profileData.ProfilePicture,
                Bio = profileData.Bio,
                Initials = $"{profileData.FirstName[0]}{profileData.LastName[0]}",

                YearsOfExperience = profileData.YearsOfExperience,
                Location = profileData.Location,
                Languages = profileData.Languages,
                JoinedDate = profileData.JoinedDate.ToString("MMMM yyyy"),

                GithubUrl = profileData.GithubUrl,
                LinkedInUrl = profileData.LinkedInUrl,
                TwitterUrl = profileData.TwitterUrl,
                WebsiteUrl = profileData.WebsiteUrl,

                Stats = new InstructorProfileStatsViewModel
                {
                    TotalCourses = profileData.TotalCourses,
                    TotalStudents = profileData.TotalStudents,
                    AverageRating = profileData.AverageRating,
                    TotalReviews = profileData.TotalReviews,
                    TeachingStreak = profileData.TeachingStreak,
                    TotalTeachingHours = profileData.TotalTeachingHours,
                    StudentInteractions = profileData.StudentInteractions,
                    CertificatesIssued = profileData.CertificatesIssued
                },

                TeachingExpertise = profileData.TeachingExpertise,

                Courses = profileData.Courses.Select((c, index) => new InstructorProfileCourseCardViewModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Status = c.Status,
                    StatusBadgeClass = c.Status.ToLower() == "published" ? "badge-green" : "badge-orange",
                    StudentsCount = c.StudentsCount,
                    Rating = c.Rating,
                    GradientStyle = gradients[index % gradients.Length]
                }).ToList()
            },

            PageTitle = "Instructor Profile"
        };

        return View(viewModel);
    }
}
