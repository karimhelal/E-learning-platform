using BLL.DTOs.Course;
using BLL.DTOs.Misc;
using BLL.Interfaces.Instructor;
using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Instructor;

public class InstructorCoursesService : IInstructorCoursesService
{
    private readonly ICourseRepository _courseRepo;
    public InstructorCoursesService(ICourseRepository courseRepo)
    {
        _courseRepo = courseRepo;
    }



    public async Task<PagedResultDto<InstructorCourseDto>> GetInstructorCoursesPagedAsync(int instructorId, PagingRequestDto request)
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
                Status = "Published",
                ThumbnailImageUrl = c.ThumbnailImageUrl,
                CreatedDate = c.CreatedDate,
                LastUpdatedDate = c.CreatedDate,
                Level = c.Level,
                MainCategory = c.Categories.FirstOrDefault(),

                // calculated fields
                NumberOfModules = c.Modules.Count(),
                NumberOfStudents = c.Enrollments!.Count(),
                NumberOfMinutes = (int)(
                                c.Modules
                                    .SelectMany(m => m.Lessons)
                                    .Select(l => l.LessonContent)
                                    .OfType<VideoContent>()
                                    .Sum(l => l.DurationInSeconds) / 60.0),
                AverageRating = 4.7f
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResultDto<InstructorCourseDto>
        {
            Items = items,

            Settings = new PaginationSettingsDto
            {
                TotalPages = totalPages,
                TotalCount = totalCount,
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize
            }
        };
    }
}
