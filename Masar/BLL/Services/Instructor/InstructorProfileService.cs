using BLL.DTOs.Instructor;
using BLL.Interfaces.Instructor;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Instructor;

public class InstructorProfileService : IInstructorProfileService
{
    private readonly IUserRepository _userRepo;
    private readonly ICourseRepository _courseRepo;

    public InstructorProfileService(IUserRepository userRepo, ICourseRepository courseRepo)
    {
        _userRepo = userRepo;
        _courseRepo = courseRepo;
    }

    public async Task<InstructorProfileDto?> GetInstructorProfileAsync(int instructorId)
    {
        var instructorProfile = await _userRepo.GetInstructorProfileAsync(instructorId, includeUserBase: true);

        if (instructorProfile == null || instructorProfile.User == null)
            return null;

        var user = instructorProfile.User;

        // Get instructor's courses
        var coursesQuery = _courseRepo.GetCoursesByInstructorQueryable(instructorId)
            .Include(c => c.Enrollments);

        var courses = await coursesQuery
            .Select(c => new InstructorProfileCourseDto
            {
                CourseId = c.Id,
                Title = c.Title,
                Description = c.Description ?? string.Empty,
                Status = "Published", // TODO: Add actual status logic
                StudentsCount = c.Enrollments!.Count(),
                Rating = 4.8f, // TODO: Calculate actual rating
                ThumbnailImageUrl = c.ThumbnailImageUrl
            })
            .ToListAsync();

        // Calculate statistics
        var totalCourses = courses.Count;
        var totalStudents = courses.Sum(c => c.StudentsCount);
        var averageRating = courses.Any() ? courses.Average(c => c.Rating) : 0;
        var totalReviews = totalStudents > 0 ? (int)(totalStudents * 0.3) : 0; // Placeholder: ~30% students leave reviews

        // Calculate teaching hours (sum of all video content duration)
        var totalTeachingHours = await coursesQuery
            .SelectMany(c => c.Modules!)
            .SelectMany(m => m.Lessons!)
            .Select(l => l.LessonContent)
            .OfType<Core.Entities.VideoContent>()
            .SumAsync(v => v.DurationInSeconds);

        var teachingHours = (int)(totalTeachingHours / 3600.0);

        return new InstructorProfileDto
        {
            InstructorId = instructorProfile.InstructorId,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber,
            ProfilePicture = user.Picture,
            Bio = instructorProfile.Bio,
            YearsOfExperience = instructorProfile.YearsOfExperience,
            
            // TODO: Add these fields to database if needed
            Location = "Cairo, Egypt", // Placeholder
            Languages = "English, Arabic", // Placeholder
            JoinedDate = DateTime.Now.AddYears(-2), // Placeholder
            
            // Social Links - TODO: Add to database
            GithubUrl = null,
            LinkedInUrl = null,
            TwitterUrl = null,
            WebsiteUrl = null,
            
            // Statistics
            TotalCourses = totalCourses,
            TotalStudents = totalStudents,
            AverageRating = averageRating,
            TotalReviews = totalReviews,
            TeachingStreak = 450, // Placeholder - implement streak calculation
            TotalTeachingHours = teachingHours,
            StudentInteractions = totalStudents * 5, // Placeholder
            CertificatesIssued = (int)(totalStudents * 0.4), // Placeholder: ~40% completion rate
            
            // Skills - TODO: Add to database or derive from courses
            TeachingExpertise = new List<string>
            {
                "React & React Native",
                "JavaScript & TypeScript",
                "Web Development",
                "Mobile Development"
            },
            
            Courses = courses
        };
    }

    public async Task<bool> UpdateInstructorProfileAsync(int instructorId, InstructorProfileDto profileDto)
    {
        var instructorProfile = await _userRepo.GetInstructorProfileAsync(instructorId, includeUserBase: true);

        if (instructorProfile == null || instructorProfile.User == null)
            return false;

        // Update instructor profile
        instructorProfile.Bio = profileDto.Bio;
        instructorProfile.YearsOfExperience = profileDto.YearsOfExperience;

        // Update user info
        instructorProfile.User.FirstName = profileDto.FirstName;
        instructorProfile.User.LastName = profileDto.LastName;
        instructorProfile.User.PhoneNumber = profileDto.Phone;

        // Save changes
        _userRepo.Update(instructorProfile.User);

        return true;
    }
}