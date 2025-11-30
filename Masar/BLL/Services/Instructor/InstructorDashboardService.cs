using BLL.DTOs.Instructor;
using BLL.Interfaces.Instructor;
using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Instructor;

public class InstructorDashboardService : IInstructorDashboardService
{
    private readonly ICourseRepository _courseRepo;
    private readonly IUserRepository _userRepo;
    public InstructorDashboardService(ICourseRepository courseRepo, IUserRepository userRepo)
    {
        _courseRepo = courseRepo;
        _userRepo = userRepo;
    }


    //public async Task<InstructorDashboardDto> GetInstructorDashboardAsync(int instructorId)
    //{
    //    var instructorProfile = await _userRepo.GetInstructorProfileAsync(instructorId, true);

    //    if (instructorProfile == null)
    //        return null!;

    //    var coursesQuery = _courseRepo.GetCoursesByInstructorQueryable(instructorId).Include(c => c.Enrollments);
    //    var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

    //    var stats = await coursesQuery
    //        .Select(c => new
    //        {
    //            StudentCount = c.Enrollments!.Count(),
    //            NewStudentsCount = c.Enrollments!.Count(e => e.EnrollmentDate!.Value >= startOfMonth),
    //            CompletionCount = c.Enrollments!.Count(e => e.ProgressPercentage >= 100)
    //        })
    //        .ToListAsync();

    //    var totalCourses = stats.Count;
    //    var totalStudent = stats.Sum(s => s.StudentCount);
    //    var newStudents = stats.Sum(s => s.NewStudentsCount);
    //    var totalCompletions = stats.Sum(s => s.CompletionCount);

    //    var completionRate = totalStudent > 0
    //        ? (double)totalCompletions / totalStudent * 100
    //        : 0;

    //    var courseCards = await coursesQuery
    //        .OrderByDescending(c => c.CreatedDate)
    //        .Take(4)
    //        .Select(c => new InstructorCourseCardDto
    //        {
    //            CourseId = c.Id,
    //            Title = c.Title,
    //            Description = c.Description!,
    //            MainCategory = c.Categories!.FirstOrDefault()!,
    //            Level = c.Level,
    //            ModulesCount = c.Modules!.Count(),
    //            StudentsCount = c.Enrollments!.Count(),
    //            Status = "Published",
    //            Rating = 4.5f,
    //            DurationHours = Math.Round(c.Modules!
    //                .SelectMany(m => m.Lessons!)
    //                .Select(l => l.LessonContent)
    //                .OfType<VideoContent>()
    //                .Sum(l => l.DurationInSeconds) / 3600.0, 2),
    //        })
    //        .ToListAsync();

    //    var topPerformingCourses = await coursesQuery
    //        .OrderByDescending(c => c.Enrollments!.Count())
    //        .Take(3)
    //        .Select(c => new InstructorTopPerformingCourseDto
    //        {
    //            Title = c.Title,
    //            StudentsEnrolled = c.Enrollments!.Count(),
    //            AverageRating = 4.9f
    //        })
    //        .ToListAsync();


    //    return new InstructorDashboardDto
    //    {
    //        InstructorId = instructorProfile.UserId,
    //        InstructorName = instructorProfile.User!.FirstName,

    //        GeneralStats = new InstructorGeneralStatsDto
    //        {
    //            TotalCourses = totalCourses,
    //            TotalStudents = totalStudent,
    //            CompletionRate = (int)completionRate,
    //            AverageRating = 4.6f // Placeholder for actual average rating calculation (yet to be added)
    //        },

    //        CurrentMonthStats = new InstructorCurrentMonthStatsDto
    //        {
    //            NewStudents = newStudents,
    //            Completions = totalCompletions,
    //            NewReviews = 26 // Placeholder for actual review count (yet to be added)
    //        },

    //        TopPerformingCourses = topPerformingCourses,
    //        CourseCards = courseCards
    //    };
    //}


    public async Task<InstructorDashboardDto> GetInstructorDashboardAsync(int instructorId)
    {
        // 1. Validate User
        var instructorProfile = await _userRepo.GetInstructorProfileQueryable(instructorId)
                                               .Include(ip => ip.User)
                                               .FirstOrDefaultAsync();

        if (instructorProfile == null)
            throw new InvalidOperationException($"Instructor with Id: {instructorId} doesn't exist.");

        // 2. Base Query
        var coursesQuery = _courseRepo.GetCoursesByInstructorQueryable(instructorId);
        var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        // 3. Aggregate Stats (Single DB Call)
        var stats = await coursesQuery
            .Select(c => new
            {
                StudentCount = c.Enrollments.Count(),
                NewStudentsCount = c.Enrollments.Count(e => e.EnrollmentDate >= startOfMonth),
                CompletionCount = c.Enrollments.Count(e => e.ProgressPercentage >= 100)
            })
            .ToListAsync();

        // 4. In-Memory Calculations
        var totalCourses = stats.Count;
        var totalStudents = stats.Sum(s => s.StudentCount);
        var totalCompletions = stats.Sum(s => s.CompletionCount);
        var completionRate = totalStudents > 0
            ? (int)((double)totalCompletions / totalStudents * 100)
            : 0;

        // 5. Course Cards
        var courseCards = await coursesQuery
            .OrderByDescending(c => c.CreatedDate)
            .Take(4)
            .Select(c => new InstructorCourseCardDto
            {
                CourseId = c.Id,
                Title = c.Title,
                Description = c.Description ?? "----",

                MainCategory = c.Categories.Select(cat => cat.Name).FirstOrDefault() ?? "UnCategorized",
                Level = c.Level.ToString(),

                ModulesCount = c.Modules.Count(),
                StudentsCount = c.Enrollments.Count(),
                
                Status = "Published",
                Rating = 4.5f,

                DurationHours = Math.Round(c.Modules
                    .SelectMany(m => m.Lessons)
                    .Select(l => l.LessonContent)
                    .OfType<VideoContent>()
                    .Sum(v => (double?)v.DurationInSeconds ?? 0) / 3600.0, 1)
            })
            .ToListAsync();

        // 6. Top Performing Courses
        var topPerforming = await coursesQuery
            .OrderByDescending(c => c.Enrollments.Count())
            .Take(3)
            .Select(c => new InstructorTopPerformingCourseDto
            {
                Title = c.Title,
                StudentsEnrolled = c.Enrollments.Count(),
                AverageRating = 4.9f
            })
            .ToListAsync();


        // 7. Return Result
        return new InstructorDashboardDto
        {
            InstructorId = instructorProfile.InstructorId,
            InstructorName = instructorProfile.User.FullName,

            GeneralStats = new InstructorGeneralStatsDto
            {
                TotalCourses = totalCourses,
                TotalStudents = totalStudents,
                CompletionRate = completionRate,
                AverageRating = 4.6f
            },

            CurrentMonthStats = new InstructorCurrentMonthStatsDto
            {
                NewStudents = stats.Sum(s => s.NewStudentsCount),
                Completions = totalCompletions,
                NewReviews = 26
            },

            TopPerformingCourses = topPerforming,
            CourseCards = courseCards
        };
    }
}
