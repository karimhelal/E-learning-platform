using BLL.DTOs.Misc;
using BLL.Helpers;
using BLL.Interfaces;
using BLL.Interfaces.Student;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using Web.Helpers;
using Web.Interfaces;
using Web.ViewModels.Misc;
using Web.ViewModels.Student;

namespace Web.Controllers.Student;

public class StudentCoursesController(
    IStudentCoursesService coursesService,
    IStudentBrowseCoursesService browseCoursesService,
    IStudentCourseDetailsService courseDetailsService,
    ICourseService courseService,
    RazorViewToStringRenderer razorRenderer,
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    AppDbContext context,
    ILogger<StudentCoursesController> logger)
    : StudentBaseController(currentUserService, userRepository, context, logger)
{
    private readonly AppDbContext _context = context;

    [HttpGet("/student/my-courses")]
    public async Task<IActionResult> MyCourses()
    {
        ViewBag.Title = "My Courses | Masar";
        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Unauthorized("Student profile not found");

        var coursesData = await coursesService.GetMyCoursesAsync(studentId);
        return coursesData == null ? NotFound("Student profile not found") : View(new StudentCoursesViewModel { Data = coursesData, PageTitle = "My Courses" });
    }

    [HttpGet("/student/browse-courses")]
    public async Task<IActionResult> BrowseCourses()
    {
        ViewBag.Title = "Browse Courses | Masar";
        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Unauthorized("Student profile not found");

        var data = await browseCoursesService.GetInitialBrowseDataAsync(studentId, new PagingRequestDto { CurrentPage = 1, PageSize = 6 });
        return data == null ? View("Error") : View(new StudentBrowseCoursesViewModel { Data = data, PageTitle = "Browse Courses" });
    }

    [HttpGet("/student/course/{courseId:int}")]
    public IActionResult CourseDetailsRedirect(int courseId) => RedirectToActionPermanent(nameof(CourseDetails), new { courseId });

    [HttpGet("/student/course/details/{courseId}")]
    public async Task<IActionResult> CourseDetails(int courseId)
    {
        ViewBag.Title = "Course Details | Masar";
        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Unauthorized("Student profile not found");

        var courseData = await courseDetailsService.GetCourseDetailsAsync(studentId, courseId);
        return courseData == null ? NotFound() : View(new StudentCourseDetailsViewModel { Data = courseData, PageTitle = courseData.Title });
    }

    [HttpPost("/student/lesson/{lessonId}/toggle")]
    public async Task<IActionResult> ToggleLessonCompletion(int lessonId, [FromBody] ToggleRequest request)
    {
        var studentId = await GetStudentIdAsync();
        if (studentId == 0) return Unauthorized();
        var result = await courseDetailsService.ToggleLessonCompletionAsync(studentId, lessonId, request.IsCompleted);
        return Json(new { success = result });
    }

    [HttpPost("/student/filter-courses")]
    public async Task<IActionResult> FilterCoursesPartial([FromBody] BrowseRequestDto request)
    {
        var studentId = await GetStudentIdAsync();
        var browseResultDto = await courseService.GetAllCoursesFilteredForBrowsingPagedAsync(request);
        var courseIds = browseResultDto.Items.Select(c => c.CourseId).ToList();

        var enrollments = await _context.CourseEnrollments
            .Where(e => e.StudentId == studentId && courseIds.Contains(e.CourseId))
            .ToDictionaryAsync(e => e.CourseId, e => e.ProgressPercentage);

        return Json(new { success = true }); 
    }
}

public class ToggleRequest { public bool IsCompleted { get; set; } }