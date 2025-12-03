using BLL.Helpers;
using BLL.Interfaces.CourseLearning;
using BLL.Interfaces.Enrollment;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Classroom;

namespace Web.Controllers.CourseLearning;

public class CourseLearningController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICourseLearningService _courseLearningService;
    private readonly RazorViewToStringRenderer _razorRenderer;
    private readonly int studentId = 1001;
    public CourseLearningController(
        IEnrollmentService enrollmentService, 
        ICourseLearningService courseLearningService, 
        RazorViewToStringRenderer razorRenderer
    )
    {
        _enrollmentService = enrollmentService;
        _courseLearningService = courseLearningService;
        _razorRenderer = razorRenderer;
    }


    [HttpGet("~/Classroom/Course/{courseId:int:required}/{lessonId:int?}")]
    public async Task<IActionResult> Index(int courseId, int? lessonId)
    {
        if (!ModelState.IsValid) {
            return NotFound(new
            {
                CourseId = ModelState["courseId"],
                LessonId = ModelState["lessonId"]
            });
        }

        // If they didn't buy it, kick them to the sales page
        if (!await _enrollmentService.IsStudentEnrolledAsync(studentId, courseId))
            return RedirectToAction("Details", "Course", new { CourseId = courseId });

        var classroomDto = await _courseLearningService.GetClassroomAsync(studentId, courseId, lessonId);

        if (classroomDto == null)
            return NotFound();

        var classroomViewModel = new CourseClassroomViewModel
        {
            CourseId = classroomDto.CourseId,
            CourseTitle = classroomDto.CourseTitle,
            Sidebar = new ClassroomSidebarViewModel
            {
                SidebarStats = new ClassroomSidebarStatsViewModel
                {
                    CourseProgress = new CourseProgressViewModel
                    {
                        CompletedLessonsCount = classroomDto.CourseProgress.CompletedLessonsCount,
                        TotalLessonsCount = classroomDto.CourseProgress.TotalLessonsCount,
                        ProgressPercentage = classroomDto.CourseProgress.ProgressPercentage
                    },

                    TotalLessonsCount = classroomDto.CourseOverviewStats.LessonsCount,
                    FormattedCourseDuration = FormatDuration(classroomDto.CourseOverviewStats.TotalDurationInMinutes),
                    FormattedNumberOfStudents = "23.5k",
                    FormattedRatingAndReviews = FormatRatingsAndReviews(classroomDto.CourseOverviewStats.AverageRating, classroomDto.CourseOverviewStats.TotalReviewsCount)
                },

                SidebarModules = classroomDto.Curriculum
                    .Select(m => new ModuleSidebarViewModel
                    {
                        ModuleId = m.ModuleId,
                        ModuleTitle = m.ModuleTitle,
                        ModuleOrder = m.ModuleOrder,
                        LessonsCount = m.LessonsCount,

                        IsCompleted = m.IsCompleted,
                        IsActive = m.IsActive,
                        FormattedModuleDuration = FormatDuration(m.TotalDurationInMinutes),

                        Lessons = m.Lessons.Select(l => new LessonSidebarViewModel
                        {
                            LessonId = l.LessonId,
                            LessonTitle = l.LessonTitle,
                            LessonOrder = l.LessonOrder,
                            ContentType = l.ContentType,
                            IsCompleted = l.IsCompleted,
                            IsActive = l.IsActive,

                            FormattedLessonDuration = FormatDuration(l.TotalDurationInMinutes)
                        })
                    }),
            },

            MainContent = new ClassroomMainContentViewModel
            {
                CourseOverviewStats = new ClassroomCourseOverviewStatsViewModel
                {
                    CourseTitle = classroomDto.CourseTitle,
                    CourseDescription = classroomDto.CourseOverviewStats.CourseDescription,
                    LessonsCount = classroomDto.CourseOverviewStats.LessonsCount,
                    ModuleCount = classroomDto.CourseOverviewStats.ModulesCount,

                    Level = classroomDto.CourseOverviewStats.Level,
                    FormattedDuration = FormatDuration(classroomDto.CourseOverviewStats.TotalDurationInMinutes),
                    FormattedRatingAndReviews = FormatRatingsAndReviews(classroomDto.CourseOverviewStats.AverageRating, classroomDto.CourseOverviewStats.TotalReviewsCount),
                    FormattedStudentsEnrolledCount = FormatCount(classroomDto.CourseOverviewStats.EnrolledStudentsCount, "Student"),

                    Categories = classroomDto.CourseOverviewStats.Categories,
                    Languages = classroomDto.CourseOverviewStats.Languages,
                    LearningOutcomes = classroomDto.CourseOverviewStats.LearningOutcomes,

                    InstructorStats = new ClassroomCourseInstructorStatsViewModel
                    {
                        InstructorId = classroomDto.CourseOverviewStats.InstructorStats.InstructorId,
                        InstructorName = classroomDto.CourseOverviewStats.InstructorStats.InstructorName,
                        Bio = classroomDto.CourseOverviewStats.InstructorStats.Bio,

                        FormattedRating = "4.3 Rating",
                        RoleTitle = "Senior Software Engineer @ Microsoft",

                        ProfilePictureUrl = classroomDto.CourseOverviewStats.InstructorStats.ProfilePictureUrl,
                        YearsOfExperience = classroomDto.CourseOverviewStats.InstructorStats.YearsOfExperience.ToString() ?? "--",
                        FormattedCoursesCount = FormatCount(classroomDto.CourseOverviewStats.InstructorStats.TotalCoursesCount, "Course"),
                        FormattedStudentsTaughtCount = FormatCount(classroomDto.CourseOverviewStats.InstructorStats.StudentsTaughtCount, "Student"),
                    }
                },

                CurrentLesson = new LessonPlayerViewModel
                {
                    LessonId = classroomDto.CurrentLesson.LessonId,
                    ModuleTitle = classroomDto.CurrentLesson.ModuleTitle,
                    
                    Metadata = new LessonMetaDataViewModel
                    {
                        LessonTitle = classroomDto.CurrentLesson.LessonTitle,
                        ContentType = classroomDto.CurrentLesson.ContentType,
                        VideoUrl = classroomDto.CurrentLesson.VideoUrl,
                        ArticleContent = classroomDto.CurrentLesson.ArticleContent
                    },
                    
                    Resources = classroomDto.CurrentLesson.Resources.Select(r => new LessonResourceViewModel
                    {
                        LessonResourceId = r.LessonResourceId,
                        ResourceTitle = r.ResourceTitle,
                        ResourceType = r.ResourceType,
                        ResourceUrl = r.ResourceUrl
                    })
                }
            }
        };


        return View("CourseViewer", classroomViewModel);
    }


    [HttpGet("Classroom/Course/GetLessonContent/{courseId:int}/{lessonId:int}")] // Changed to GET
    public async Task<IActionResult> GetLessonContentPartialAsync(int courseId, int lessonId)
    {
        // 1. Get Student ID (Assuming Helper Method)
        //var studentId = GetStudentId();

        // 2. Security Gate: AJAX Friendly
        if (!await _enrollmentService.IsStudentEnrolledAsync(studentId, courseId))
        {
            // Return 403 so JS knows to redirect the whole window
            return StatusCode(403, new { message = "Access Denied. Please purchase the course." });
        }

        // 3. Fetch Data
        var lessonDataDto = await _courseLearningService.GetLessonDataAsync(studentId, lessonId);

        if (lessonDataDto == null) 
            return NotFound("Lesson data not found");

        // 4. Map to ViewModel
        var viewModel = new LessonPlayerViewModel
        {
            LessonId = lessonDataDto.LessonId,
            ModuleTitle = lessonDataDto.ModuleTitle,
            // We need these titles for the JSON response later, keep them handy

            Metadata = new LessonMetaDataViewModel
            {
                LessonTitle = lessonDataDto.LessonTitle,
                ContentType = lessonDataDto.ContentType,
                ArticleContent = lessonDataDto.ArticleContent,
                VideoUrl = lessonDataDto.VideoUrl
            },

            Resources = lessonDataDto.Resources.Select(r => new LessonResourceViewModel
            {
                LessonResourceId = r.LessonResourceId,
                ResourceTitle = r.ResourceTitle,
                ResourceType = r.ResourceType,
                ResourceUrl = r.ResourceUrl
            })
        };

        // 5. Render Partials to String
        // Passing the specific sub-models (Metadata and Resources)
        string lessonMetaDataHtml = await _razorRenderer.RenderViewToStringAsync(
            ControllerContext,
            "_ClassroomLessonPlayerPartialView",
            viewModel.Metadata
        );

        string lessonResourcesHtml = await _razorRenderer.RenderViewToStringAsync(
            ControllerContext,
            "_ClassroomLessonResourcesPartialView",
            viewModel.Resources
        );

        // 6. Return JSON with Metadata
        return Json(new
        {
            // Metadata for JS updates
            lessonId = viewModel.LessonId,
            moduleTitle = lessonDataDto.ModuleTitle,
            isCompleted = lessonDataDto.IsCompleted,

            // The HTML to inject
            playerHtml = lessonMetaDataHtml,
            resourcesHtml = lessonResourcesHtml
        });
    }


    [HttpPost("~/Classroom/Course/UpdateLessonCompletionState/{courseId:int:required}/{lessonId:int:required}/{newCompletionState:bool=false}")]
    public async Task<IActionResult> UpdateLessonCompletionStatePartialAsync(int courseId, int lessonId, bool newCompletionState)
    {
        // 1. Fetch Data
        var newCourseProgressDto = await _courseLearningService.UpdateLessonCompletionState(studentId, lessonId, newCompletionState);

        if (newCourseProgressDto == null)
            return StatusCode(403, new { message = "Access Denied. Please purchase the course." });

        return Json(new
        {
            Success = true,
            FormattedProgressSubtitle = $"{newCourseProgressDto.CompletedLessonsCount} of {newCourseProgressDto.TotalLessonsCount} lessons",
            ProgressPercentage = newCourseProgressDto.ProgressPercentage
        });
    }


    [HttpPost("~/Classroom/Course/MarkLessonStarted/{lessonId:int:required}")]
    public async Task<IActionResult> MarkLessonStarted(int lessonId)
    {
        // 1. Security Gate: AJAX Friendly
        var courseId = await _enrollmentService.GetCourseIdIfEnrolled(studentId, lessonId);
        
        // Return 403 so JS knows to redirect the whole window
        if (courseId == null)
            return StatusCode(403, new { message = "Access Denied, you don't have access to the requested resource." });

        var success = await _courseLearningService.MarkLessonStartedAsync(studentId, lessonId);

        return Json(new
        {
            Success = success,
        });
    }



    private string FormatDuration(int durationInMinutes)
    {
        int h = durationInMinutes / 60;
        int m = durationInMinutes % 60;

        List<string> parts = new();

        if (h > 0)
            parts.Add($"{h} {(h == 1 ? "hr" : "hrs")}");

        if (m > 0)
            parts.Add($"{m} {(m == 1 ? "min" : "mins")}");

        return parts.Count > 0 ? string.Join(" ", parts) : "--";
    }

    private string FormatRatingsAndReviews(float averageRating, int totalReviewsCount)
    {
        string rating = averageRating.ToString("0.0");
        string reviewsCount = totalReviewsCount.ToString("N0"); // adds commas
        string reviewWord = totalReviewsCount == 1 ? "review" : "reviews";

        //return $"{rating} ({reviewsCount} {reviewWord})";
        return $"{rating}";
    }

    private string FormatCount(int count, string singularWord)
    {
        string number = count.ToString("N0");
        string word = count == 1 ? singularWord : singularWord + "s";

        return $"{number} {word}";
    }
}
