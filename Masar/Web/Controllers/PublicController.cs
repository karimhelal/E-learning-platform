using BLL.Interfaces;
using Core.RepositoryInterfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Interfaces;
using Web.ViewModels.Public;

namespace Web.Controllers;

public class PublicController : Controller
{
    private readonly IPublicInstructorService _publicInstructorService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public PublicController(
        IPublicInstructorService publicInstructorService,
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        _publicInstructorService = publicInstructorService;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Public Instructor Profile - View instructor profile (accessible to students and guests)
    /// </summary>
    [HttpGet("/instructor/{instructorId:int}")]
    public async Task<IActionResult> InstructorProfile(int instructorId)
    {
        var profileData = await _publicInstructorService.GetInstructorPublicProfileAsync(instructorId);

        if (profileData == null)
        {
            return NotFound("Instructor not found");
        }

        // Get current user info for layout if logged in
        int? studentId = null;
        string? studentName = null;
        string? userInitials = null;

        var userId = _currentUserService.GetUserId();
        if (userId > 0)
        {
            var studentProfile = await _userRepository.GetStudentProfileForUserAsync(userId, includeUserBase: true);
            if (studentProfile?.User != null)
            {
                studentId = studentProfile.StudentId;
                studentName = studentProfile.User.FirstName;
                userInitials = $"{studentProfile.User.FirstName[0]}{studentProfile.User.LastName[0]}".ToUpper();
            }
        }

        var gradients = new[]
        {
            "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
            "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
            "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)",
            "linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)",
            "linear-gradient(135deg, #fa709a 0%, #fee140 100%)",
            "linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)"
        };

        var viewModel = new PublicInstructorProfileViewModel
        {
            Data = new PublicInstructorProfileDataViewModel
            {
                InstructorId = profileData.InstructorId,
                FirstName = profileData.FirstName,
                LastName = profileData.LastName,
                FullName = profileData.FullName,
                Username = $"@{profileData.FirstName.ToLower()}_{profileData.LastName.ToLower()}",
                ProfilePicture = profileData.ProfilePicture,
                Bio = profileData.Bio,
                Initials = $"{profileData.FirstName[0]}{profileData.LastName[0]}".ToUpper(),

                YearsOfExperience = profileData.YearsOfExperience,
                JoinedDate = profileData.JoinedDate.ToString("MMMM yyyy"),

                GithubUrl = profileData.GithubUrl,
                LinkedInUrl = profileData.LinkedInUrl,
                WebsiteUrl = profileData.WebsiteUrl,

                Stats = new PublicInstructorStatsViewModel
                {
                    TotalCourses = profileData.TotalCourses,
                    TotalStudents = profileData.TotalStudents,
                    AverageRating = profileData.AverageRating,
                    TotalReviews = profileData.TotalReviews
                },

                Skills = profileData.Skills,

                Courses = profileData.Courses.Select((c, index) => new PublicInstructorCourseViewModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    ThumbnailImageUrl = c.ThumbnailImageUrl,
                    Level = c.Level,
                    LevelBadgeClass = c.Level.ToLower(),
                    CategoryName = c.CategoryName,
                    CategoryBadgeClass = GetCategoryBadgeClass(c.CategoryName),
                    StudentsCount = c.StudentsCount,
                    Rating = c.Rating,
                    TotalLessons = c.TotalLessons,
                    DurationHours = c.DurationHours,
                    GradientStyle = gradients[index % gradients.Length]
                }).ToList()
            },

            PageTitle = $"{profileData.FullName} | Instructor Profile",
            StudentId = studentId,
            StudentName = studentName,
            UserInitials = userInitials
        };

        ViewBag.Title = viewModel.PageTitle;

        return View("InstructorProfile", viewModel);
    }

    private static string GetCategoryBadgeClass(string categoryName)
    {
        return categoryName.ToLower() switch
        {
            "web development" => "badge-purple",
            "data science" => "badge-cyan",
            "mobile development" => "badge-green",
            "design" => "badge-pink",
            _ => "badge-purple"
        };
    }
}