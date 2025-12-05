using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;
using Web.Interfaces;
using Web.ViewModels.Student;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Web.Services
{
    /// <summary>
    /// Service for retrieving all tracks available for students (Browse Tracks)
    /// Maps from Student ID to User ID for profile lookup
    /// </summary>
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

        /// <summary>
        /// Gets all available tracks for browsing along with student profile info
        /// </summary>
        /// <param name="studentId">The student profile ID (1-7 from seeded data)</param>
        /// <returns>Browse tracks page data with student info and available tracks</returns>
        public async Task<StudentBrowseTracksPageData?> GetAllTracksAsync(int studentId)
        {
            try
            {
                _logger.LogInformation("Fetching browse tracks for student: {StudentId}", studentId);

                // Load student profile for name and initials
                var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);
                
                var studentName = "Student";
                var userInitials = "JD";
                
                if (studentProfile?.User != null)
                {
                    studentName = studentProfile.User.FirstName;
                    userInitials = GetInitials($"{studentProfile.User.FirstName} {studentProfile.User.LastName}");
                }

                // Fetch actual tracks from database
                var tracks = await _context.Tracks
                    .Include(t => t.TrackCourses)
                        .ThenInclude(tc => tc.Course)
                    .Include(t => t.Enrollments)
                    .Include(t => t.Categories)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} tracks in database", tracks.Count);

                var mappedTracks = tracks.Select(t => MapTrack(t, studentId)).ToList();

                return new StudentBrowseTracksPageData
                {
                    StudentId = studentId,
                    StudentName = studentName,
                    UserInitials = userInitials,
                    
                    Stats = new BrowseTrackPageStats
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

        private BrowseTrackItem MapTrack(Track track, int studentId)
        {
            var courses = track.TrackCourses?
                .Where(tc => tc.Course != null)
                .Select(tc => tc.Course!)
                .ToList() ?? new List<Course>();

            var coursesCount = courses.Count;
            var studentsCount = track.Enrollments?.Count() ?? 0;
            
            // Calculate total duration
            var totalHours = courses
                .Where(c => c.Modules != null)
                .SelectMany(c => c.Modules!)
                .SelectMany(m => m.Lessons ?? new List<Lesson>())
                .Select(l => l.LessonContent)
                .OfType<VideoContent>()
                .Sum(v => v.DurationInSeconds) / 3600;

            // Get category
            var categoryName = track.Categories?.FirstOrDefault()?.Name ?? "Learning Track";
            
            // Check if student is enrolled
            var isEnrolled = track.Enrollments?.Any(e => e.StudentId == studentId) ?? false;

            return new BrowseTrackItem
            {
                TrackId = track.Id,
                Title = track.Title,
                Description = track.Description ?? "Explore this learning track",
                Level = "Beginner", // TODO: Add level to Track entity
                CategoryName = categoryName,
                CategoryIcon = GetCategoryIcon(categoryName),
                LevelBadgeClass = "beginner",
                CoursesCount = coursesCount,
                DurationHours = (int)totalHours,
                StudentsCount = studentsCount,
                Rating = 4.8m,
                Skills = new List<string>(), // TODO: Extract from courses
                CoursesPreview = courses.Take(3).Select(c => new CoursePreview
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

        /// <summary>
        /// Helper method to generate user initials from full name
        /// </summary>
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
}
