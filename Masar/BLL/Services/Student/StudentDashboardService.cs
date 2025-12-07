using BLL.DTOs.Student;
using BLL.Interfaces.Student;
using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Student;

public class StudentDashboardService : IStudentDashboardService
{
    private readonly ICourseRepository _courseRepo;
    private readonly IUserRepository _userRepo;
    public StudentDashboardService(ICourseRepository courseRepo, IUserRepository userRepo)
    {
        _courseRepo = courseRepo;
        _userRepo = userRepo;
    }

    public async Task<StudentDashboardDto?> GetStudentDashboardAsync(int studentId)
    {
        if (!_userRepo.HasStudentProfileWithId(studentId))
            throw new InvalidOperationException($"Invalid Opertion: Student with Id: {studentId} doesn't exist\nMake sure you are not passing user id for student id");

        var studentProfileQuery = _userRepo.GetStudentProfileQueryable(studentId)!;

        var dashboardDto = await studentProfileQuery.Select(sp => new StudentDashboardDto
        {
            StudentId = sp.StudentId,
            UserId = sp.UserId,
            StudentName = sp.User!.FullName,

            // --- General Stats ---
            GeneralStats = new StudentGeneralStatsDto
            {
                ActiveTracks = sp.Enrollments.OfType<TrackEnrollment>().Count(),
                EnrolledCourses = sp.Enrollments.OfType<CourseEnrollment>().Count(),
                CompletedCourses = sp.Enrollments
                    .OfType<CourseEnrollment>()
                    .Count(e => e.ProgressPercentage >= 100),
                CertificatesEarned = sp.Certificates.Count()
            },

            // --- Continue Learning (Courses) ---
            ContinueLearningCourses = sp.Enrollments!.OfType<CourseEnrollment>()
                .Select(e => new StudentContinueLearningCourseCardDto
                {
                    CourseId = e.CourseId,
                    Title = e.Course!.Title,
                    Description = e.Course.Description ?? "---",
                    ThumbnailImageUrl = e.Course.ThumbnailImageUrl,

                    InstructorName = e.Course.Instructor.User.FullName ?? "????",

                    MainCategoryName = e.Course.Categories.First().Name ?? "Uncategorized",
                    LevelName = e.Course.Level.ToString(),
                    LevelClass = e.Course.Level.ToString().ToLower(),

                    DurationHours = (int)Math.Round(e.Course.Modules!
                        .SelectMany(m => m.Lessons!)
                        .Select(l => l.LessonContent)
                        .OfType<VideoContent>()
                        .Sum(v => (double?)v.DurationInSeconds) ?? 0 / 3600.0),

                    ProgressPercentage = e.ProgressPercentage
                })
                .ToList(),


            EnrolledTracks = sp.Enrollments!.OfType<TrackEnrollment>()
                .Select(e => new StudentEnrolledTrackCardDto
                {
                    TrackId = e.TrackId,
                    Title = e.Track!.Title,
                    Description = e.Track.Description ?? "---",

                    CoursesCount = e.Track.Courses.Count(),
                    TotalHours = (int)Math.Round(e.Track.Courses
                        .SelectMany(c => c.Modules)                            // Flatten all modules from all courses
                        .SelectMany(m => m.Lessons)                            // Flatten all lessons
                        .Select(l => l.LessonContent)
                        .OfType<VideoContent>()
                        .Sum(v => (double?)v.DurationInSeconds) ?? 0 / 3600.0),

                    ProgressPercentage = e.ProgressPercentage
                })
                .ToList()
        })
        .FirstOrDefaultAsync();


        return dashboardDto;
    }
}
