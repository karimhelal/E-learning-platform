using BLL.DTOs.Course;
using BLL.DTOs.Course.Lesson;
using BLL.DTOs.Course.Module;
using BLL.DTOs.Instructor;
using BLL.DTOs.Misc;
using BLL.Interfaces;
using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepo;
    public CourseService(ICourseRepository courseRepo) => _courseRepo = courseRepo;


    public async Task<PagedResult<InstructorCourseDto>> GetCoursesByInctructorAsync(
        int instructorId, 
        PagingRequest request)
    {
        var query = _courseRepo.GetCoursesByInstructorQueryable(instructorId);

        bool isASC = request.SortOrder.ToUpper() == "ASC";
        string sortBy = request.SortBy?.ToLower() ?? "createdDate";

        query = sortBy switch
        {
            "createdDate" => isASC ? query.OrderBy(c => c.CreatedDate)
                                   : query.OrderByDescending(c => c.CreatedDate),

            "students" => isASC ? query.OrderBy(c => c.Enrollments!.Count())
                                : query.OrderByDescending(c => c.Enrollments!.Count()),

            _ => query.OrderByDescending(c => c.CreatedDate)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.CurrentPage - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new InstructorCourseDto
            {
                CourseId = c.Id,
                Title = c.Title,
                Description = c.Description,
                ThumbnailImageUrl = c.ThumbnailImageUrl,
                CreatedDate = c.CreatedDate,
                MainCategory = c.Categories.FirstOrDefault(),

                // calculated fields
                NumberOfModules = c.Modules.Count(),
                NumberOfStudents = c.Enrollments!.Count(),
                NumberOfMinutes = (int)(
                                c.Modules
                                    .SelectMany(m => m.Lessons)
                                    .Select(l => l.LessonContent)
                                    .OfType<VideoContent>()
                                    .Sum(l => l.DurationInSeconds) / 60.0)
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResult<InstructorCourseDto>
        {
            Items = items,
            TotalPages = totalCount,
            CurrentPage = request.CurrentPage,
            PageSize = request.PageSize
        };
    }

    public async Task<InstructorCourseEditResult> GetInstructorCourseForEditorAsync(
        int courseId,
        int instructorId,
        InstructorCourseEditRequest request)
    {
        return new InstructorCourseEditResult();
    }

    public InstructorCourseBasicDetailsDto? GetInstructorCourseBasicDetails(
        int instructorId,
        int courseId)
    {
        var course = _courseRepo.GetByIdAsync(courseId).Result;

        if (course == null)
            return null;

        return new InstructorCourseBasicDetailsDto
        {
            InstructorId = instructorId,

            CourseId = course.Id,
            Title = course.Title,
            Description = course.Description!,
            Level = course.Level,
            ThumbnailImageUrl = course.ThumbnailImageUrl!,
            MainCategory = course.Categories?.FirstOrDefault()!,
            AdiitionalCategories = course.Categories!,
            LearningOutcomes = course.LearningOutcomes,
        };
    }

    public InstructorCourseContentDetailsDto? GetInstructorCourseContentDetails(
        int instructorId, 
        int courseId)
    {
        var query = _courseRepo.GetCourseByIdQueryable(courseId)
            .Where(c => c.InstructorId == instructorId)
            .Select(c => new InstructorCourseContentDetailsDto
            {
                CourseId = c.Id,
                InstructorId = c.InstructorId,

                ModulesDetails = c.Modules!.Select(m => new ModuleDetailsDto
                {
                    ModuleId = m.ModuleId,
                    CourseId = m.CourseId,
                    ModuleName = m.Title,
                    ModuleOrder = m.Order,

                    LessonsDetails = m.Lessons!.Select(l => new LessonDetailsDto 
                    {
                        LessonId = l.LessonId,
                        LessonName = l.Title,
                        LessonOrder = l.Order,
                        LessonContentType = l.ContentType,
                        ModuleId = m.ModuleId,

                        DurationInSeconds = l.LessonContent is VideoContent 
                            ? ((VideoContent)l.LessonContent).DurationInSeconds 
                            : 0
                    }),

                    TotalDurationInSeconds = m.Lessons!.Sum(l => 
                        l.LessonContent is VideoContent
                        ? ((VideoContent)l.LessonContent).DurationInSeconds
                        : 0)
                }),
            });

        return query.FirstOrDefault();
    }

    public IQueryable<InstructorTopPerformingCourseDto> GetInstructorTopPerformingCourses(
        int instructorId, 
        int topN)
    {
        var query = _courseRepo.GetCoursesByInstructorQueryable(instructorId);

        var result = query.Take(topN).Select(c => new InstructorTopPerformingCourseDto
        {
            Title = c.Title,
            StudentsEnrolled = c.Enrollments!.Count(),
            AverageRating = (float)Math.Round(Math.Max(3.2, new Random().NextDouble() * 5.0), 2)
        });

        return result;
    }
}
