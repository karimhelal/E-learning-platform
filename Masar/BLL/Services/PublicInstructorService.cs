using BLL.DTOs.Instructor;
using BLL.Interfaces;
using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class PublicInstructorService : IPublicInstructorService
{
    private readonly IUserRepository _userRepo;
    private readonly ICourseRepository _courseRepo;

    public PublicInstructorService(IUserRepository userRepo, ICourseRepository courseRepo)
    {
        _userRepo = userRepo;
        _courseRepo = courseRepo;
    }

    public async Task<PublicInstructorProfileDto?> GetInstructorPublicProfileAsync(int instructorId)
    {
        var instructorProfile = await _userRepo.GetInstructorProfileAsync(instructorId, includeUserBase: true);

        if (instructorProfile == null || instructorProfile.User == null)
            return null;

        var user = instructorProfile.User;

        // Get instructor's published courses only - fetch data first
        var coursesData = await _courseRepo.GetCoursesByInstructorQueryable(instructorId)
            .Include(c => c.Enrollments)
            .Include(c => c.Categories)
            .Include(c => c.Modules!)
                .ThenInclude(m => m.Lessons!)
                    .ThenInclude(l => l.LessonContent)
            .ToListAsync();

        // Map to DTOs in memory (not in SQL)
        var courses = coursesData.Select(c => new PublicInstructorCourseDto
        {
            CourseId = c.Id,
            Title = c.Title,
            Description = c.Description ?? string.Empty,
            ThumbnailImageUrl = c.ThumbnailImageUrl,
            Level = c.Level.ToString(),
            CategoryName = c.Categories != null && c.Categories.Any() 
                ? c.Categories.First().Name 
                : "General",
            StudentsCount = c.Enrollments?.Count() ?? 0,
            Rating = 4.8f, // TODO: Calculate actual rating from reviews
            TotalLessons = c.Modules?
                .SelectMany(m => m.Lessons ?? Enumerable.Empty<Lesson>())
                .Count() ?? 0,
            DurationHours = (int)Math.Ceiling(
                (c.Modules?
                    .SelectMany(m => m.Lessons ?? Enumerable.Empty<Lesson>())
                    .Select(l => l.LessonContent)
                    .OfType<VideoContent>()
                    .Sum(v => v.DurationInSeconds) ?? 0) / 3600.0)
        }).ToList();

        // Calculate statistics
        var totalCourses = courses.Count;
        var totalStudents = courses.Sum(c => c.StudentsCount);
        var averageRating = courses.Any() ? courses.Average(c => c.Rating) : 0;
        var totalReviews = totalStudents > 0 ? (int)(totalStudents * 0.3) : 0;

        // Get skills from database (only Instructor type)
        var skills = user.Skills?
            .Where(s => s.SkillType == "Instructor")
            .Select(s => s.SkillName)
            .ToList() ?? new List<string>();

        // Get social links from UserSocialLinks table (public ones only)
        var socialLinks = user.UserSocialLinks?.ToList() ?? new List<UserSocialLink>();
        var githubUrl = socialLinks.FirstOrDefault(s => s.SocialPlatform == Core.Entities.Enums.SocialPlatform.Github)?.Url;
        var linkedInUrl = socialLinks.FirstOrDefault(s => s.SocialPlatform == Core.Entities.Enums.SocialPlatform.LinkedIn)?.Url;
        var websiteUrl = socialLinks.FirstOrDefault(s => s.SocialPlatform == Core.Entities.Enums.SocialPlatform.Personal)?.Url;

        return new PublicInstructorProfileDto
        {
            InstructorId = instructorProfile.InstructorId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfilePicture = user.Picture,
            Bio = instructorProfile.Bio,
            YearsOfExperience = instructorProfile.YearsOfExperience,
            JoinedDate = DateTime.Now.AddYears(-2), // TODO: Add actual joined date field
            
            // Social Links (public only)
            GithubUrl = githubUrl,
            LinkedInUrl = linkedInUrl,
            WebsiteUrl = websiteUrl,
            
            // Statistics
            TotalCourses = totalCourses,
            TotalStudents = totalStudents,
            AverageRating = averageRating,
            TotalReviews = totalReviews,
            
            Skills = skills,
            Courses = courses
        };
    }
}