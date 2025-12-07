using BLL.DTOs.Instructor.ManageCourse;
using BLL.Interfaces.Instructor;
using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BLL.Services.Instructor;

public class InstructorManageCourseService : IInstructorManageCourseService
{
    private readonly ICourseRepository _courseRepo;
    private readonly IInstructorProfileRepository _instructorProfileRepo;
    private readonly ILessonRepository _lessonRepo;
    public InstructorManageCourseService (ICourseRepository courseRepository, IInstructorProfileRepository instructorProfileRepository, ILessonRepository lessonRepository)
    {
        _courseRepo = courseRepository;
        _instructorProfileRepo = instructorProfileRepository;
        _lessonRepo = lessonRepository;
    }

    public async Task<ManageViewCourseDto?> GetCourseForManageAsync(int instructorId, int courseId)
    {
        var courseQuery = _courseRepo.GetCourseByIdQueryable(courseId)
            .Where(c => c.InstructorId == instructorId);

        if (!await courseQuery.AnyAsync())
            return null;

        var courseManageData = await courseQuery
            .AsSplitQuery()
            .Select(c => new ManageViewCourseDto
            {
                CourseId = c.Id,
                CourseTitle = c.Title,
                CourseStatus = c.Status,
                Curriculum = new ManageViewCourseCurriculumDto
                {
                    CourseId = c.Id,
                    CourseTitle = c.Title,
                    CourseStatus = c.Status,

                    Modules = c.Modules.OrderBy(m => m.Order).Select(m => new ManageViewModuleDto
                    {
                        ModuleId = m.ModuleId,
                        ModuleTitle = m.Title,
                        ModuleOrder = m.Order,
                        LessonsCount = m.Lessons.Count(),
                        
                        Lessons = m.Lessons.OrderBy(l => l.Order).Select(l => new ManageViewLessonDto
                        {
                            LessonId = l.LessonId,
                            LessonTitle = l.Title,
                            LessonOrder = l.Order,
                            ContentType = l.ContentType,

                            DurationInMinutes = (l.LessonContent is VideoContent) ? (int)Math.Round(((VideoContent)l.LessonContent).DurationInSeconds / 60.0) : null
                        })
                    })
                }
            })
            .FirstOrDefaultAsync();

        // calculate module duration in-memory
        foreach(var module in courseManageData.Curriculum.Modules)
        {
            var videoLessonsDurations = module.Lessons
                .Select(l => l.DurationInMinutes)
                .Where(duration => duration != null);

            if (!videoLessonsDurations.Any())
                module.DurationInMinutes = null;

            module.DurationInMinutes = videoLessonsDurations.Sum();
        }

        return courseManageData;
    }


    public async Task<ManageViewCourseCurriculumDto?> GetCourseCurriculumAsync(int instructorId, int courseId)
    {
        var courseQuery = _courseRepo.GetCourseByIdQueryable(courseId)
            .Where(c => c.InstructorId == instructorId);

        if (!await courseQuery.AnyAsync())
            return null;

        var courseCurriculumDto = await courseQuery.Select(c => new ManageViewCourseCurriculumDto
        {
            CourseId = c.Id,
            CourseTitle = c.Title,
            CourseStatus = c.Status,

            Modules = c.Modules.OrderBy(m => m.Order).Select(m => new ManageViewModuleDto
            {
                ModuleId = m.ModuleId,
                ModuleTitle = m.Title,
                ModuleOrder = m.Order,
                LessonsCount = m.Lessons.Count(),

                Lessons = m.Lessons.OrderBy(l => l.Order).Select(l => new ManageViewLessonDto
                {
                    LessonId = l.LessonId,
                    LessonTitle = l.Title,
                    LessonOrder = l.Order,
                    ContentType = l.ContentType,

                    DurationInMinutes = (l.LessonContent is VideoContent) ? (int)Math.Round(((VideoContent)l.LessonContent).DurationInSeconds / 60.0) : null
                })
            })
        })
        .FirstOrDefaultAsync();

        return courseCurriculumDto;
    }

    public async Task<ManageEditLessonDto?> GetLessonDataAsync(int instructorId, int lessonId)
    {
        var lessonDataDto = await _lessonRepo.GetByIdQueryable(lessonId)
            .Where(l => l.Module.Course.InstructorId == instructorId)
            .Select(l => new ManageEditLessonDto
            {
                LessonId = l.LessonId,
                LessonTitle = l.Title,
                LessonOrder = l.Order,

                ContentType = l.ContentType,

                DurationInMinutes = (l.LessonContent is VideoContent)
                    ? (int?)Math.Round(((VideoContent)l.LessonContent).DurationInSeconds / 60.0)
                    : null,

                VideoUrl = (l.LessonContent is VideoContent)
                    ? ((VideoContent)l.LessonContent).VideoUrl
                    : null,

                ArticleContent = (l.LessonContent is ArticleContent)
                    ? ((ArticleContent)l.LessonContent).Content
                    : null,

                Resources = l.LessonResources.Select(r => new ManageEditLessonResourceDto
                {
                    LessonResourceId = r.LessonResourceId,
                    ResourceType = r.ResourceKind,
                    Title = r.Title,
                    Url = r.Url
                })
            })
            .FirstOrDefaultAsync();

        return lessonDataDto;
    }
}
