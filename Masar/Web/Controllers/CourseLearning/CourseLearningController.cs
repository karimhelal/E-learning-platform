using AutoMapper;
using BLL.Helpers;
using BLL.Interfaces.CourseLearning;
using BLL.Interfaces.Enrollment;
using Microsoft.AspNetCore.Mvc;
using Web.Interfaces;
using Web.ViewModels.Classroom;

namespace Web.Controllers.CourseLearning;

public class CourseLearningController(
    IEnrollmentService _enrollmentService,
    ICourseLearningService _courseLearningService,
    ICurrentUserService _currentUserService,
    RazorViewToStringRenderer _razorRenderer,
    IMapper _mapper) : Controller
{
    [HttpGet("~/Classroom/Course/{courseId:int:required}/{lessonId:int?}")]
    public async Task<IActionResult> Index(int courseId, int? lessonId)
    {
        if (!ModelState.IsValid)
            return NotFound(new { CourseId = ModelState["courseId"], LessonId = ModelState["lessonId"] });

        var studentId = _currentUserService.GetStudentId();
        if (!await _enrollmentService.IsStudentEnrolledAsync(studentId, courseId))
            return RedirectToAction("Details", "Course", new { CourseId = courseId });

        var classroomDto = await _courseLearningService.GetClassroomAsync(studentId, courseId, lessonId);
        if (classroomDto == null) return NotFound();

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
                    FormattedCourseDuration = $"{classroomDto.CourseOverviewStats.TotalDurationInMinutes / 60} hrs {classroomDto.CourseOverviewStats.TotalDurationInMinutes % 60} mins",
                    FormattedNumberOfStudents = "23.5k",
                    FormattedRatingAndReviews = $"{classroomDto.CourseOverviewStats.AverageRating:0.0}"
                },
                // الماپينج السحري للموديولات والدروس!
                SidebarModules = _mapper.Map<List<ModuleSidebarViewModel>>(classroomDto.Curriculum)
            },
            MainContent = new ClassroomMainContentViewModel
            {
                CourseOverviewStats = _mapper.Map<ClassroomCourseOverviewStatsViewModel>(classroomDto.CourseOverviewStats),
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
                    Resources = _mapper.Map<List<LessonResourceViewModel>>(classroomDto.CurrentLesson.Resources)
                }
            }
        };

        return View("CourseViewer", classroomViewModel);
    }

    [HttpGet("Classroom/Course/GetLessonContent/{courseId:int}/{lessonId:int}")]
    public async Task<IActionResult> GetLessonContentPartialAsync(int courseId, int lessonId)
    {
        var studentId = _currentUserService.GetStudentId();

        if (!await _enrollmentService.IsStudentEnrolledAsync(studentId, courseId))
            return StatusCode(403, new { message = "Access Denied. Please purchase the course." });

        var lessonDataDto = await _courseLearningService.GetLessonDataAsync(studentId, lessonId);
        if (lessonDataDto == null) return NotFound("Lesson data not found");

        var viewModel = new LessonPlayerViewModel
        {
            LessonId = lessonDataDto.LessonId,
            ModuleTitle = lessonDataDto.ModuleTitle,
            Metadata = new LessonMetaDataViewModel
            {
                LessonTitle = lessonDataDto.LessonTitle,
                ContentType = lessonDataDto.ContentType,
                ArticleContent = lessonDataDto.ArticleContent,
                VideoUrl = lessonDataDto.VideoUrl
            },
            Resources = _mapper.Map<List<LessonResourceViewModel>>(lessonDataDto.Resources)
        };

        string lessonMetaDataHtml = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_ClassroomLessonPlayerPartialView", viewModel.Metadata);
        string lessonResourcesHtml = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_ClassroomLessonResourcesPartialView", viewModel.Resources);

        return Json(new { lessonId = viewModel.LessonId, moduleTitle = lessonDataDto.ModuleTitle, isCompleted = lessonDataDto.IsCompleted, playerHtml = lessonMetaDataHtml, resourcesHtml = lessonResourcesHtml });
    }

    [HttpPost("~/Classroom/Course/UpdateLessonCompletionState/{courseId:int:required}/{lessonId:int:required}/{newCompletionState:bool=false}")]
    public async Task<IActionResult> UpdateLessonCompletionStatePartialAsync(int courseId, int lessonId, bool newCompletionState)
    {
        var studentId = _currentUserService.GetStudentId();
        var newCourseProgressDto = await _courseLearningService.UpdateLessonCompletionState(studentId, lessonId, newCompletionState);

        if (newCourseProgressDto == null)
            return StatusCode(403, new { message = "Access Denied. Please purchase the course." });

        return Json(new { Success = true, FormattedProgressSubtitle = $"{newCourseProgressDto.CompletedLessonsCount} of {newCourseProgressDto.TotalLessonsCount} lessons", ProgressPercentage = newCourseProgressDto.ProgressPercentage });
    }

    [HttpPost("~/Classroom/Course/MarkLessonStarted/{lessonId:int:required}")]
    public async Task<IActionResult> MarkLessonStarted(int lessonId)
    {
        var studentId = _currentUserService.GetStudentId();
        var courseId = await _enrollmentService.GetCourseIdIfEnrolled(studentId, lessonId);

        if (courseId == null) return StatusCode(403, new { message = "Access Denied, you don't have access to the requested resource." });

        var success = await _courseLearningService.MarkLessonStartedAsync(studentId, lessonId);
        return Json(new { Success = success });
    }
}