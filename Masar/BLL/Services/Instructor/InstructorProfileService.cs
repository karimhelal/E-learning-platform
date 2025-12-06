using BLL.DTOs.Instructor;
using BLL.Interfaces.Instructor;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Instructor;

public class InstructorProfileService : IInstructorProfileService
{
    private readonly IUserRepository _userRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly IUnitOfWork _unitOfWork;

    public InstructorProfileService(IUserRepository userRepo, ICourseRepository courseRepo, IUnitOfWork unitOfWork)
    {
        _userRepo = userRepo;
        _courseRepo = courseRepo;
        _unitOfWork = unitOfWork;
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

        // Get skills from database (only Instructor type)
        var skills = user.Skills?
            .Where(s => s.SkillType == "Instructor")
            .Select(s => s.SkillName)
            .ToList() ?? new List<string>();

        // Get social links from UserSocialLinks table
        var socialLinks = user.UserSocialLinks?.ToList() ?? new List<Core.Entities.UserSocialLink>();
        var githubUrl = socialLinks.FirstOrDefault(s => s.SocialPlatform == Core.Entities.Enums.SocialPlatform.Github)?.Url;
        var linkedInUrl = socialLinks.FirstOrDefault(s => s.SocialPlatform == Core.Entities.Enums.SocialPlatform.LinkedIn)?.Url;
        var facebookUrl = socialLinks.FirstOrDefault(s => s.SocialPlatform == Core.Entities.Enums.SocialPlatform.Facebook)?.Url;
        var websiteUrl = socialLinks.FirstOrDefault(s => s.SocialPlatform == Core.Entities.Enums.SocialPlatform.Personal)?.Url;

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
            JoinedDate = DateTime.Now.AddYears(-2), // TODO: Add actual joined date field
            
            // Social Links from database
            GithubUrl = githubUrl,
            LinkedInUrl = linkedInUrl,
            FacebookUrl = facebookUrl,
            WebsiteUrl = websiteUrl,
            
            // Statistics
            TotalCourses = totalCourses,
            TotalStudents = totalStudents,
            AverageRating = averageRating,
            TotalReviews = totalReviews,
            TeachingStreak = 450, // Placeholder - implement streak calculation
            TotalTeachingHours = teachingHours,
            StudentInteractions = totalStudents * 5, // Placeholder
            CertificatesIssued = (int)(totalStudents * 0.4), // Placeholder: ~40% completion rate
            
            // Skills from database (renamed from TeachingExpertise)
            Skills = skills,
            
            Courses = courses
        };
    }

    public async Task<bool> UpdateInstructorProfileAsync(int instructorId, UpdateInstructorProfileDto profileDto)
    {
        var instructorProfile = await _userRepo.GetInstructorProfileAsync(instructorId, includeUserBase: true);

        if (instructorProfile == null || instructorProfile.User == null)
            return false;

        // Update instructor profile fields (only Bio and YearsOfExperience are persisted in DB currently)
        instructorProfile.Bio = profileDto.Bio;
        instructorProfile.YearsOfExperience = profileDto.YearsOfExperience;
        
        // Update user info
        instructorProfile.User.FirstName = profileDto.FirstName;
        instructorProfile.User.LastName = profileDto.LastName;
        instructorProfile.User.PhoneNumber = profileDto.Phone;

        // Update skills (with SkillType = Instructor)
        if (profileDto.Skills != null)
        {
            // Remove existing instructor skills only
            var existingInstructorSkills = instructorProfile.User.Skills?
                .Where(s => s.SkillType == "Instructor")
                .ToList() ?? new List<Core.Entities.Skill>();
            
            foreach (var skill in existingInstructorSkills)
            {
                instructorProfile.User.Skills?.Remove(skill);
            }
            
            // Add new skills with SkillType = Instructor
            foreach (var skillName in profileDto.Skills.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                instructorProfile.User.Skills?.Add(new Core.Entities.Skill
                {
                    SkillName = skillName.Trim(),
                    UserId = instructorProfile.UserId,
                    SkillType = "Instructor"
                });
            }
        }

        // Update social links
        instructorProfile.User.UserSocialLinks?.Clear();
        instructorProfile.User.UserSocialLinks = new List<Core.Entities.UserSocialLink>();

        if (!string.IsNullOrEmpty(profileDto.GithubUrl))
        {
            instructorProfile.User.UserSocialLinks.Add(new Core.Entities.UserSocialLink
            {
                Url = profileDto.GithubUrl,
                SocialPlatform = Core.Entities.Enums.SocialPlatform.Github,
                UserId = instructorProfile.UserId
            });
        }

        if (!string.IsNullOrEmpty(profileDto.LinkedInUrl))
        {
            instructorProfile.User.UserSocialLinks.Add(new Core.Entities.UserSocialLink
            {
                Url = profileDto.LinkedInUrl,
                SocialPlatform = Core.Entities.Enums.SocialPlatform.LinkedIn,
                UserId = instructorProfile.UserId
            });
        }

        if (!string.IsNullOrEmpty(profileDto.FacebookUrl))
        {
            instructorProfile.User.UserSocialLinks.Add(new Core.Entities.UserSocialLink
            {
                Url = profileDto.FacebookUrl,
                SocialPlatform = Core.Entities.Enums.SocialPlatform.Facebook,
                UserId = instructorProfile.UserId
            });
        }

        if (!string.IsNullOrEmpty(profileDto.WebsiteUrl))
        {
            instructorProfile.User.UserSocialLinks.Add(new Core.Entities.UserSocialLink
            {
                Url = profileDto.WebsiteUrl,
                SocialPlatform = Core.Entities.Enums.SocialPlatform.Personal,
                UserId = instructorProfile.UserId
            });
        }

        // Save changes using Unit of Work
        await _unitOfWork.CompleteAsync();

        return true;
    }
}