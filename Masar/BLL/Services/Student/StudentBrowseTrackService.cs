namespace BLL.Services.Student;

using BLL.DTOs.Student;
using BLL.Interfaces.Student;
using Core.Entities;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class StudentBrowseTrackService : IStudentBrowseTrackService
{
    private readonly IUserRepository _userRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<StudentBrowseTrackService> _logger;

    public StudentBrowseTrackService(
        IUserRepository userRepo,
        AppDbContext context,
        ILogger<StudentBrowseTrackService> logger)
    {
        _userRepo = userRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<StudentBrowseTracksPageDto?> GetAllTracksAsync(int studentId)
    {
        try
        {
            var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);
            var studentName = "Student";
            var userInitials = "JD";

            if (studentProfile?.User != null)
            {
                studentName = studentProfile.User.FirstName;
                userInitials = GetInitials($"{studentProfile.User.FirstName} {studentProfile.User.LastName}");
            }

            var tracks = await _context.Tracks
                .Include(t => t.TrackCourses)
                    .ThenInclude(tc => tc.Course)
                        .ThenInclude(c => c!.Modules)
                            .ThenInclude(m => m.Lessons)
                                .ThenInclude(l => l.LessonContent)
                .Include(t => t.Enrollments)
                .Include(t => t.Categories)
                .ToListAsync();

            var mappedTracks = tracks.Select(t => MapTrack(t, studentId)).ToList();

            return new StudentBrowseTracksPageDto
            {
                StudentId = studentId,
                StudentName = studentName,
                UserInitials = userInitials,
                Stats = new BrowseTrackPageStatsDto
                {
                    TotalTracks = mappedTracks.Count,
                    BeginnerTracks = mappedTracks.Count(t => t.Level == "Beginner"),
                    IntermediateTracks = mappedTracks.Count(t => t.Level == "Intermediate"),
                    AdvancedTracks = mappedTracks.Count(t => t.Level == "Advanced")
                },
                Tracks = mappedTracks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading browse tracks for student {StudentId}", studentId);
            return null;
        }
    }

    private BrowseTrackItemDto MapTrack(Track track, int studentId)
    {
        var courses = track.TrackCourses?
            .Where(tc => tc.Course != null)
            .Select(tc => tc.Course!)
            .ToList() ?? new List<Course>();

        var totalSeconds = courses
            .Where(c => c.Modules != null)
            .SelectMany(c => c.Modules!)
            .SelectMany(m => m.Lessons ?? new List<Lesson>())
            .Select(l => l.LessonContent)
            .OfType<VideoContent>()
            .Sum(v => v.DurationInSeconds);

        var totalHours = (int)Math.Ceiling(totalSeconds / 3600.0);
        var categoryName = track.Categories?.FirstOrDefault()?.Name ?? "Learning Track";
        var isEnrolled = track.Enrollments?.Any(e => e.StudentId == studentId) ?? false;

        var skills = courses
            .SelectMany(c => c.Categories ?? new List<Category>())
            .Select(cat => cat.Name)
            .Distinct()
            .Take(4)
            .ToList();

        return new BrowseTrackItemDto
        {
            TrackId = track.Id,
            Title = track.Title,
            Description = track.Description ?? "Explore this learning track",
            Level = "Beginner",
            CategoryName = categoryName,
            CategoryIcon = GetCategoryIcon(categoryName),
            LevelBadgeClass = "beginner",
            CoursesCount = courses.Count,
            DurationHours = totalHours,
            StudentsCount = track.Enrollments?.Count() ?? 0,
            Rating = 4.8m,
            Skills = skills,
            CoursesPreview = courses.Take(3).Select(c => new CoursePreviewDto
            {
                CourseId = c.Id,
                Title = c.Title,
                Difficulty = c.Level.ToString()
            }).ToList(),
            ActionText = isEnrolled ? "Continue Learning" : "View Details",
            ActionUrl = $"/track/{track.Id}"
        };
    }

    private string GetCategoryIcon(string categoryName)
    {
        return categoryName.ToLower() switch
        {
            "web development" => "fa-laptop-code",
            "data science" => "fa-brain",
            "mobile development" => "fa-mobile-alt",
            "design" => "fa-palette",
            _ => "fa-book"
        };
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "JD";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}{parts[1][0]}".ToUpper() : parts[0][0].ToString().ToUpper();
    }
}