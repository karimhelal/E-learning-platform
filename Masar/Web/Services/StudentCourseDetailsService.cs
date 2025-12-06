using BLL.DTOs.Student;
using Core.Entities;
using Core.Entities.Enums;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Web.Interfaces;

namespace Web.Services;

public class StudentCourseDetailsService : IStudentCourseDetailsService
{
    private readonly ICourseRepository _courseRepo;
    private readonly IGenericRepository<CourseEnrollment> _enrollmentRepo;
    private readonly IGenericRepository<LessonProgress> _progressRepo;
    private readonly AppDbContext _context;
    private readonly ICertificateGenerationService _certificateService;

    public StudentCourseDetailsService(
        ICourseRepository courseRepo,
        IGenericRepository<CourseEnrollment> enrollmentRepo,
        IGenericRepository<LessonProgress> progressRepo,
        AppDbContext context,
        ICertificateGenerationService certificateService)
    {
        _courseRepo = courseRepo;
        _enrollmentRepo = enrollmentRepo;
        _progressRepo = progressRepo;
        _context = context;
        _certificateService = certificateService;
    }

    public async Task<StudentCourseDetailsDto?> GetCourseDetailsAsync(int studentId, int courseId)
    {
        // Get course with all relationships
        var course = await _courseRepo.GetCourseByIdQueryable(courseId)
            .Include(c => c.Instructor!)
                .ThenInclude(i => i.User)
            .Include(c => c.Categories)
            .Include(c => c.LearningOutcomes)
            .Include(c => c.Modules!.OrderBy(m => m.Order))
                .ThenInclude(m => m.Lessons!.OrderBy(l => l.Order))
                    .ThenInclude(l => l.LessonContent)
            .Include(c => c.Modules!)
                .ThenInclude(m => m.Assignments)
            .Include(c => c.Enrollments!.Where(e => e.StudentId == studentId))
            .FirstOrDefaultAsync();

        if (course == null) return null;

        // Get student enrollment
        var enrollment = course.Enrollments?.FirstOrDefault();
        if (enrollment == null) return null;

        // GET STUDENT INFO (ADD THIS)
        var studentProfile = await _context.StudentProfiles
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.StudentId == studentId);
        
        if (studentProfile?.User == null) return null;

        var studentFirstName = studentProfile.User.FirstName ?? "Student";
        var studentLastName = studentProfile.User.LastName ?? "";
        var studentInitials = GetInitials($"{studentFirstName} {studentLastName}");

        // Get all lesson progress for this student
        var allLessonIds = course.Modules?
            .SelectMany(m => m.Lessons ?? Enumerable.Empty<Lesson>())
            .Select(l => l.LessonId)
            .ToList() ?? new List<int>();

        var lessonProgressList = await _progressRepo.GetAllQueryable()
            .Where(lp => lp.StudentId == studentId && allLessonIds.Contains(lp.LessonId))
            .ToListAsync();

        var completedLessonIds = lessonProgressList
            .Where(lp => lp.IsCompleted)
            .Select(lp => lp.LessonId)
            .ToHashSet();

        // Calculate total duration (from VideoContent only)
        var totalDurationSeconds = course.Modules?
            .SelectMany(m => m.Lessons ?? Enumerable.Empty<Lesson>())
            .Where(l => l.LessonContent is VideoContent)
            .Sum(l => ((VideoContent)l.LessonContent).DurationInSeconds) ?? 0;

        // Build modules with progress
        var modulesDto = course.Modules?.OrderBy(m => m.Order).Select(module =>
        {
            var lessons = module.Lessons?.OrderBy(l => l.Order).ToList() ?? new List<Lesson>();

            var moduleDuration = lessons
                .Where(l => l.LessonContent is VideoContent)
                .Sum(l => ((VideoContent)l.LessonContent).DurationInSeconds);

            var completedInModule = lessons.Count(l => completedLessonIds.Contains(l.LessonId));

            var lessonsDto = lessons.Select(lesson =>
            {
                var isCompleted = completedLessonIds.Contains(lesson.LessonId);

                var duration = lesson.LessonContent is VideoContent video
                    ? (int)Math.Ceiling(video.DurationInSeconds / 60.0)
                    : 10; // Default for articles

                // FIX: Use FirstOrDefault() instead of FirstOrDefaultAsync() since lessonProgressList is already a List
                var completedDate = lessonProgressList
                    .FirstOrDefault(lp => lp.LessonId == lesson.LessonId && lp.IsCompleted)?
                    .CompletedDate;

                return new LessonWithProgressDto
                {
                    LessonId = lesson.LessonId,
                    LessonOrder = lesson.Order,
                    LessonName = lesson.Title,
                    ContentType = lesson.ContentType,
                    DurationMinutes = duration,
                    IsCompleted = isCompleted,
                    CompletedDate = completedDate
                };
            }).ToList();

            return new ModuleWithProgressDto
            {
                ModuleId = module.ModuleId,
                ModuleOrder = module.Order,
                ModuleName = module.Title,
                ModuleDescription = module.Description ?? "",
                TotalLessons = lessons.Count,
                CompletedLessons = completedInModule,
                DurationMinutes = (int)Math.Ceiling(moduleDuration / 60.0),
                AssignmentsCount = module.Assignments?.Count ?? 0,
                ProgressPercentage = lessons.Count > 0 ? (decimal)completedInModule / lessons.Count * 100 : 0,
                Lessons = lessonsDto
            };
        }).ToList() ?? new List<ModuleWithProgressDto>();

        var totalLessons = modulesDto.Sum(m => m.TotalLessons);
        var completedLessons = modulesDto.Sum(m => m.CompletedLessons);

        // Get track info using TrackCourses
        var trackInfo = await _context.Set<TrackEnrollment>()
            .Where(te => te.StudentId == studentId)
            .Where(te => te.Track!.TrackCourses!.Any(tc => tc.CourseId == courseId))
            .Select(te => new { te.TrackId, te.Track!.Title })
            .FirstOrDefaultAsync();

        // Get instructor name from User entity
        var firstName = course.Instructor?.User?.FirstName ?? "Instructor";
        var lastName = course.Instructor?.User?.LastName ?? "Name";
        var initials = $"{firstName[0]}{lastName[0]}".ToUpper();

        return new StudentCourseDetailsDto
        {
            // ADD THESE TWO LINES
            StudentName = studentFirstName,
            UserInitials = studentInitials,
            
            CourseId = course.Id,
            Title = course.Title,
            Description = course.Description ?? "",
            ThumbnailImageUrl = course.ThumbnailImageUrl ?? "",
           // Language = course.Language,
            Level = course.Level,
            CategoryName = course.Categories?.FirstOrDefault()?.Name ?? "General",
            InstructorName = $"{firstName} {lastName}",
            InstructorInitials = initials,
            TotalModules = course.Modules?.Count ?? 0,
            TotalLessons = totalLessons,
            TotalDurationMinutes = (int)Math.Ceiling(totalDurationSeconds / 60.0),
            TotalStudentsEnrolled = course.Enrollments?.Count ?? 0,
            ProgressPercentage = enrollment.ProgressPercentage,
            CompletedLessons = completedLessons,
            EnrollmentDate = enrollment.EnrollmentDate,
            EnrollmentStatus = enrollment.Status.ToString(),
            TrackId = trackInfo?.TrackId,
            TrackName = trackInfo?.Title ?? "",
            LearningOutcomes = course.LearningOutcomes?
                .Select(lo => new LearningOutcomeDto
                {
                    Id = lo.Id,
                    Title = lo.Title,
                    Description = lo.Description
                })
                .ToList() ?? new List<LearningOutcomeDto>(),
            Modules = modulesDto
        };
    }

    public async Task<bool> ToggleLessonCompletionAsync(int studentId, int lessonId, bool isCompleted)
    {
        var progress = await _progressRepo.GetAllQueryable()
            .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LessonId == lessonId);

        if (progress == null && isCompleted)
        {
            progress = new LessonProgress
            {
                StudentId = studentId,
                LessonId = lessonId,
                IsCompleted = true,
                StartedDate = DateTime.UtcNow,
                CompletedDate = DateTime.UtcNow
            };
            await _progressRepo.AddAsync(progress);
        }
        else if (progress != null)
        {
            progress.IsCompleted = isCompleted;
            progress.CompletedDate = isCompleted ? DateTime.UtcNow : null;
            _progressRepo.Update(progress);
        }

        // Save lesson progress changes
        await _context.SaveChangesAsync();

        // Update course enrollment progress percentage
        await UpdateCourseProgressAsync(studentId, lessonId);

        return true;
    }

    private async Task UpdateCourseProgressAsync(int studentId, int lessonId)
    {
        // Get the lesson and its course
        var lesson = await _context.Lessons
            .Include(l => l.Module)
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);

        if (lesson?.Module?.CourseId == null) return;

        var courseId = lesson.Module.CourseId;

        // Get all lessons in the course
        var allLessonsInCourse = await _context.Modules
            .Where(m => m.CourseId == courseId)
            .SelectMany(m => m.Lessons!)
            .Select(l => l.LessonId)
            .ToListAsync();

        // Get completed lessons count
        var completedLessonsCount = await _context.Set<LessonProgress>()
            .Where(lp => lp.StudentId == studentId 
                      && allLessonsInCourse.Contains(lp.LessonId) 
                      && lp.IsCompleted)
            .CountAsync();

        // Calculate progress percentage
        var totalLessons = allLessonsInCourse.Count;
        var progressPercentage = totalLessons > 0 
            ? (decimal)completedLessonsCount / totalLessons * 100 
            : 0;

        // Update enrollment
        var enrollment = await _context.Set<CourseEnrollment>()
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

        if (enrollment != null)
        {
            enrollment.ProgressPercentage = progressPercentage;
            
            // Update status based on progress
            if (progressPercentage >= 100)
            {
                enrollment.Status = EnrollmentStatus.Completed;
                
                // AUTOMATIC CERTIFICATE GENERATION
                await _certificateService.GenerateCourseCertificateAsync(studentId, courseId);
                
                // Also check if this completes any track
                await CheckAndGenerateTrackCertificatesAsync(studentId, courseId);
            }
            else if (progressPercentage > 0)
            {
                enrollment.Status = EnrollmentStatus.InProgress;
            }

            _context.Update(enrollment);
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckAndGenerateTrackCertificatesAsync(int studentId, int courseId)
    {
        // Find all tracks that include this course
        var trackIds = await _context.Set<Track_Course>()
            .Where(tc => tc.CourseId == courseId)
            .Select(tc => tc.TrackId)
            .ToListAsync();

        foreach (var trackId in trackIds)
        {
            // Check if student is enrolled in this track
            var trackEnrollment = await _context.TrackEnrollments
                .FirstOrDefaultAsync(te => te.StudentId == studentId && te.TrackId == trackId);

            if (trackEnrollment != null)
            {
                // Try to generate track certificate (service will check if all courses are complete)
                await _certificateService.GenerateTrackCertificateAsync(studentId, trackId);
            }
        }
    }

    // ADD THIS HELPER METHOD at the end of the class
    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "JD";

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();

        return parts[0][0].ToString().ToUpper();
    }
}