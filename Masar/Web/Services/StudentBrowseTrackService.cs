using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;
using Web.Interfaces;
using Web.ViewModels.Student;

namespace Web.Services
{
    /// <summary>
    /// Service for retrieving all tracks available for students (Browse Tracks)
    /// Maps from Student ID to User ID for profile lookup
    /// </summary>
    public class StudentBrowseTrackService : IStudentBrowseTrackService
    {
        private readonly IUserRepository _userRepo;
        private readonly ILogger<StudentBrowseTrackService> _logger;

        public StudentBrowseTrackService(
            IUserRepository userRepo,
            ILogger<StudentBrowseTrackService> logger)
        {
            _userRepo = userRepo;
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
                // studentId maps to StudentProfile IDs (1-7)
                var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);
                
                var studentName = "Student";
                var userInitials = "JD";
                
                if (studentProfile?.User != null)
                {
                    studentName = studentProfile.User.FirstName;
                    userInitials = GetInitials($"{studentProfile.User.FirstName} {studentProfile.User.LastName}");
                }

                // TODO: Fetch actual tracks from ITrackRepository
                // For now, return empty tracks list to avoid errors
                var mappedTracks = new List<BrowseTrackItem>();

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
