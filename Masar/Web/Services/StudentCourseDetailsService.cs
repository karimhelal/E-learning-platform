using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;
using Web.Interfaces;
using Microsoft.EntityFrameworkCore;
using DAL.Data;

namespace Web.Services;

public class StudentCourseDetailsService : IStudentCourseDetailsService
{
    private readonly IUserRepository _userRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<StudentCourseDetailsService> _logger;

    public StudentCourseDetailsService(
        IUserRepository userRepo,
        ICourseRepository courseRepo,
        AppDbContext context,
        ILogger<StudentCourseDetailsService> logger)
    {
        _userRepo = userRepo;
        _courseRepo = courseRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<StudentCourseDetailsData?> GetCourseDetailsAsync(int userId, int courseId)
    {
        try
        {
            _logger.LogInformation("📚 Fetching course details for user {UserId}, course {CourseId}", userId, courseId);

            // Get student profile
            var studentProfile = await _userRepo.GetStudentProfileForUserAsync(userId, includeUserBase: true);
            
            if (studentProfile == null)
            {
                _logger.LogWarning("⚠️ Student profile not found for user {UserId}", userId);
                return null;
            }

            if (studentProfile.User == null)
            {
                _logger.LogWarning("⚠️ User data not found for student profile");
                return null;
            }

            // Find enrollment
            var enrollment = studentProfile.Enrollments?
                .OfType<CourseEnrollment>()
                .FirstOrDefault(e => e.CourseId == courseId);

            if (enrollment == null)
            {
                _logger.LogWarning("⚠️ No enrollment found for user {UserId} in course {CourseId}", userId, courseId);
                return null;
            }

            var course = enrollment.Course;
            if (course == null)
            {
                _logger.LogWarning("⚠️ Course data not loaded for course {CourseId}", courseId);
                return null;
            }

            // Get lesson progress
            var lessonProgressList = await _context.LessonProgress
                .Where(lp => lp.StudentId == studentProfile.StudentId)
                .ToListAsync();

            var lessonProgressDict = lessonProgressList.ToDictionary(lp => lp.LessonId, lp => lp.IsCompleted);

            // Map modules and lessons
            var modules = course.Modules?
                .OrderBy(m => m.Order)
                .Select(m => new ModuleDetailsItem
                {
                    ModuleId = m.ModuleId,
                    Title = m.Title,
                    Description = m.Description ?? string.Empty,
                    Order = m.Order,
                    Lessons = m.Lessons?
                        .OrderBy(l => l.Order)
                        .Select(l => new LessonDetailsItem
                        {
                            LessonId = l.LessonId,
                            Title = l.Title,
                            Order = l.Order,
                            ContentType = l.ContentType.ToString(),
                            DurationSeconds = l.LessonContent is VideoContent vc ? vc.DurationInSeconds : 0,
                            IsCompleted = lessonProgressDict.GetValueOrDefault(l.LessonId, false)
                        })
                        .ToList() ?? new List<LessonDetailsItem>()
                })
                .ToList() ?? new List<ModuleDetailsItem>();

            var totalLessons = modules.Sum(m => m.Lessons.Count);
            var completedLessons = modules.Sum(m => m.Lessons.Count(l => l.IsCompleted));

            var instructorName = course.Instructor?.User != null
                ? $"{course.Instructor.User.FirstName} {course.Instructor.User.LastName}"
                : "Unknown Instructor";

            // Get student name and initials
            var studentName = $"{studentProfile.User.FirstName} {studentProfile.User.LastName}";
            var userInitials = GetInitials($"{studentProfile.User.FirstName} {studentProfile.User.LastName}");

            var result = new StudentCourseDetailsData
            {
                CourseId = course.Id,
                Title = course.Title,
                Description = course.Description ?? string.Empty,
                StudentName = studentName,
                UserInitials = userInitials,  // ✅ ADD THIS LINE
                InstructorName = instructorName,
                CategoryName = course.Categories?.FirstOrDefault()?.Name ?? "General",
                TotalModules = modules.Count,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                ProgressPercentage = enrollment.ProgressPercentage,
                Modules = modules
            };

            _logger.LogInformation("✅ Course details retrieved successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting course details for user {UserId}, course {CourseId}", userId, courseId);
            return null;
        }
    }

    public async Task<bool> ToggleLessonCompletionAsync(int userId, int lessonId, bool isCompleted)
    {
        try
        {
            _logger.LogInformation("🔄 Toggling lesson {LessonId} completion for user {UserId}", lessonId, userId);

            var studentProfile = await _userRepo.GetStudentProfileForUserAsync(userId, includeUserBase: false);
            
            if (studentProfile == null)
            {
                _logger.LogWarning("⚠️ Student profile not found");
                return false;
            }

            // Get or create lesson progress
            var lessonProgress = await _context.LessonProgress
                .FirstOrDefaultAsync(lp => lp.StudentId == studentProfile.StudentId && lp.LessonId == lessonId);

            if (lessonProgress == null)
            {
                lessonProgress = new LessonProgress
                {
                    StudentId = studentProfile.StudentId,
                    LessonId = lessonId,
                    IsCompleted = isCompleted,
                    StartedDate = DateTime.Now,
                    CompletedDate = isCompleted ? DateTime.Now : null
                };
                _context.LessonProgress.Add(lessonProgress);
            }
            else
            {
                lessonProgress.IsCompleted = isCompleted;
                lessonProgress.CompletedDate = isCompleted ? DateTime.Now : null;
            }

            // ✅ RECALCULATE COURSE PROGRESS
            // Get the lesson to find which course it belongs to
            var lesson = await _context.Lessons
                .Include(l => l.Module)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson != null && lesson.Module != null)
            {
                var courseId = lesson.Module.CourseId;

                // Get all lessons in this course
                var allCourseLessons = await _context.Modules
                    .Where(m => m.CourseId == courseId)
                    .SelectMany(m => m.Lessons)
                    .Select(l => l.LessonId)
                    .ToListAsync();

                // Get completed lessons count
                var completedLessonsCount = await _context.LessonProgress
                    .Where(lp => lp.StudentId == studentProfile.StudentId && 
                                allCourseLessons.Contains(lp.LessonId) && 
                                lp.IsCompleted)
                    .CountAsync();

                // Calculate new progress percentage
                var totalLessons = allCourseLessons.Count;
                var newProgressPercentage = totalLessons > 0 
                    ? (decimal)completedLessonsCount / totalLessons * 100 
                    : 0;

                // ✅ UPDATE ENROLLMENT PROGRESS
                var enrollment = await _context.Set<CourseEnrollment>()
                    .FirstOrDefaultAsync(e => e.StudentId == studentProfile.StudentId && 
                                             e.CourseId == courseId);

                if (enrollment != null)
                {
                    enrollment.ProgressPercentage = Math.Round(newProgressPercentage, 2);
                    
                    // Update status based on progress
                    if (enrollment.ProgressPercentage >= 100)
                    {
                        enrollment.Status = Core.Entities.Enums.EnrollmentStatus.Completed;
                    }
                    else if (enrollment.ProgressPercentage > 0)
                    {
                        enrollment.Status = Core.Entities.Enums.EnrollmentStatus.InProgress;
                    }

                    _logger.LogInformation("📊 Updated course {CourseId} progress to {Progress}%", 
                        courseId, enrollment.ProgressPercentage);
                }
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("✅ Lesson {LessonId} marked as {Status}", 
                lessonId, isCompleted ? "completed" : "incomplete");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error toggling lesson completion");
            return false;
        }
    }

    // ✅ ADD THIS HELPER METHOD
    private string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "ST";

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        
        if (parts.Length == 1 && parts[0].Length >= 2)
            return parts[0].Substring(0, 2).ToUpper();

        return parts[0][0].ToString().ToUpper();
    }
}