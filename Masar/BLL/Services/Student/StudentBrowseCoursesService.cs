namespace BLL.Services.Student;

using BLL.DTOs.Misc;
using BLL.DTOs.Student;
using BLL.Interfaces; // For ICourseService
using BLL.Interfaces.Student;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class StudentBrowseCoursesService : IStudentBrowseCoursesService
{
    private readonly IUserRepository _userRepo;
    private readonly ICourseService _courseService;
    private readonly AppDbContext _context;
    private readonly ILogger<StudentBrowseCoursesService> _logger;

    public StudentBrowseCoursesService(
        IUserRepository userRepo,
        ICourseService courseService,
        AppDbContext context,
        ILogger<StudentBrowseCoursesService> logger)
    {
        _userRepo = userRepo;
        _courseService = courseService;
        _context = context;
        _logger = logger;
    }

    public async Task<StudentBrowseCoursesPageDto?> GetInitialBrowseDataAsync(int studentId, PagingRequestDto pagingRequest)
    {
        try
        {
            _logger.LogInformation("Fetching browse courses for student: {StudentId}", studentId);

            // Load student profile for name and initials
            var studentProfile = await _userRepo.GetStudentProfileAsync(studentId, includeUserBase: true);

            var studentName = "Student";
            var userInitials = "JD";

            if (studentProfile?.User != null)
            {
                studentName = studentProfile.User.FirstName;
                userInitials = GetInitials($"{studentProfile.User.FirstName} {studentProfile.User.LastName}");
            }

            // Get courses data using the course service
            var coursesData = await _courseService.GetInitialBrowsePageCoursesAsync(pagingRequest);

            // Get all course IDs from the result
            var courseIds = coursesData.Items.Select(c => c.CourseId).ToList();

            // Get enrollment status for all courses in one query
            var enrollments = await _context.CourseEnrollments
                .Where(e => e.StudentId == studentId && courseIds.Contains(e.CourseId))
                .Select(e => new { e.CourseId, e.ProgressPercentage })
                .ToDictionaryAsync(e => e.CourseId, e => e.ProgressPercentage);


            var filterGroups = await BuildBrowseFilterGroupsAsync();

            var items = coursesData.Items.Select(c => new StudentCourseBrowseCardDto
            {
                CourseId = c.CourseId,
                InstructorName = c.InstructorName,
                Title = c.Title,
                Description = c.Description ?? "--",
                ThumbnailImageUrl = c.ThumbnailImageUrl ?? "",
                CreatedDate = c.CreatedDate.ToString("dd-MM-yyyy"),
                MainCategory = c.MainCategory ?? "Uncategorized",
                Categories = c.Categories?.ToList() ?? new List<string>(),
                Languages = c.Languages?.ToList() ?? new List<string>(),
                Level = c.Level.ToString(),
                AverageRating = (decimal)c.AverageRating,
                NumberOfReviews = c.NumberOfReviews,
                NumberOfStudents = c.NumberOfStudents,
                NumberOfLectures = c.NumberOfLectures,
                NumberOfMinutes = c.NumberOfMinutes,
                IsEnrolled = enrollments.ContainsKey(c.CourseId),
                ProgressPercentage = enrollments.TryGetValue(c.CourseId, out var progress) ? progress : 0
            });

            return new StudentBrowseCoursesPageDto
            {
                StudentId = studentId,
                StudentName = studentName,
                UserInitials = userInitials,
                Settings = new StudentBrowseSettingsDto
                {
                    FilterGroups = filterGroups,
                    PaginationSettings = new BLL.DTOs.Misc.PaginationSettingsDto
                    {
                        CurrentPage = coursesData.Settings.PaginationSettings.CurrentPage,
                        TotalPages = coursesData.Settings.PaginationSettings.TotalPages,
                        PageSize = coursesData.Settings.PaginationSettings.PageSize,
                        TotalCount = coursesData.Settings.PaginationSettings.TotalCount
                    }
                },
                Items = items.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading browse courses for student {StudentId}", studentId);
            return null;
        }
    }

    private async Task<List<FilterGroupDto>> BuildBrowseFilterGroupsAsync()
    {
        var filterGroups = await _courseService.GetFilterSectionConfig();
        var filterGroupsStats = await _courseService.GetFilterGroupsStats();

        var result = new List<FilterGroupDto>();


        return result;
    }

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