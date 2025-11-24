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
    private readonly RazorViewToStringRenderer _razorRenderer;

    public InstructorController(
        IInstructorDashboardService dashboardService, 
        IInstructorCoursesService courseService, 
        RazorViewToStringRenderer razorRenderer)
    {
        _dashboardService = dashboardService;
        _coursesService = courseService;
        _razorRenderer = razorRenderer;
    }


    [HttpGet("/instructor/dashboard")]
    [HttpGet("/")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Title = "Instructor Dashboard | Masar";

        var instructorId = 5;     // TODO: Get from logged in user context
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
        
        var userId = 5;     // TODO: Get from logged in user context
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
        var userId = 5;     // TODO: Get from logged in user context
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
}
