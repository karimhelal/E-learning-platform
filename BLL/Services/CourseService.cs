using BLL.DTOs.Course;
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
                Id = c.Id,
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
            TotalCount = totalCount,
            CurrentPage = request.CurrentPage,
            PageSize = request.PageSize
        };
    }
}
