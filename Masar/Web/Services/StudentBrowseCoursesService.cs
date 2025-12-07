using Core.RepositoryInterfaces;
using Microsoft.Extensions.Logging;
using Web.Interfaces;
using Web.ViewModels.Student;
using Web.ViewModels.Home;
using Web.ViewModels.Misc;
using Web.ViewModels.Misc.FilterRequestVMs;
using BLL.Interfaces;
using BLL.DTOs.Misc;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace Web.Services
{
    /// <summary>
    /// Service for retrieving all courses available for students (Browse Courses)
    /// </summary>
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

        public async Task<StudentBrowseCoursesPageData?> GetInitialBrowseDataAsync(int studentId, PagingRequestDto pagingRequest)
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

                // Build filter groups
                var filterGroups = await BuildBrowseFilterGroupsAsync();

                var items = coursesData.Items.Select(c => new StudentCourseBrowseCardViewModel
                {
                    CourseId = c.CourseId,
                    InstructorName = c.InstructorName,
                    Title = c.Title,
                    Description = c.Description ?? "--",
                    ThumbnailImageUrl = c.ThumbnailImageUrl ?? "",
                    CreatedDate = c.CreatedDate.ToString("dd-MM-yyyy"),
                    MainCategory = c.MainCategory ?? "Uncategorized",
                    Categories = c.Categories ?? [],
                    Languages = c.Languages ?? [],
                    Level = c.Level.ToString(),
                    AverageRating = c.AverageRating,
                    NumberOfReviews = c.NumberOfReviews,
                    NumberOfStudents = c.NumberOfStudents,
                    NumberOfLectures = c.NumberOfLectures,
                    NumberOfMinutes = c.NumberOfMinutes,
                    // Enrollment status
                    IsEnrolled = enrollments.ContainsKey(c.CourseId),
                    ProgressPercentage = enrollments.TryGetValue(c.CourseId, out var progress) ? progress : 0
                });

                return new StudentBrowseCoursesPageData
                {
                    StudentId = studentId,
                    StudentName = studentName,
                    UserInitials = userInitials,
                    Settings = new BrowseSettingsViewModel
                    {
                        FilterGroups = filterGroups,
                        PaginationSettings = new PaginationSettingsViewModel
                        {
                            CurrentPage = coursesData.Settings.PaginationSettings.CurrentPage,
                            TotalPages = coursesData.Settings.PaginationSettings.TotalPages,
                            PageSize = coursesData.Settings.PaginationSettings.PageSize,
                            TotalCount = coursesData.Settings.PaginationSettings.TotalCount
                        }
                    },
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading browse courses for student {StudentId}", studentId);
                return null;
            }
        }

        private async Task<List<FilterGroupViewModel>> BuildBrowseFilterGroupsAsync()
        {
            var filterGroups = await _courseService.GetFilterSectionConfig();
            var filterGroupsStats = await _courseService.GetFilterGroupsStats();

            var result = new List<FilterGroupViewModel>();

            if (filterGroupsStats.CategoryCounts.Any())
            {
                result.Add(new CheckboxFilter
                {
                    Title = "Categories",
                    RequestKey = "CategoryNames",
                    FilterOptions = filterGroups.CategoryNames!
                        .Where(c => filterGroupsStats.CategoryCounts.SingleOrDefault(e => e.Key.ToLower() == c.ToLower()).Value > 0)
                        .Select(cat => new FilterOption
                        {
                            Label = cat,
                            Count = filterGroupsStats.CategoryCounts.SingleOrDefault(e => e.Key.ToLower() == cat.ToLower()).Value,
                            IsChecked = false,
                            Value = cat
                        })
                        .OrderByDescending(x => x.Count)
                });
            }

            if (filterGroupsStats.LevelCounts.Any())
            {
                result.Add(new CheckboxFilter
                {
                    Title = "Levels",
                    RequestKey = "LevelNames",
                    FilterOptions = filterGroups.LevelNames!
                        .Where(l => filterGroupsStats.LevelCounts.SingleOrDefault(e => e.Key.ToLower() == l.ToLower()).Value > 0)
                        .Select(lev => new FilterOption
                        {
                            Label = lev,
                            Count = filterGroupsStats.LevelCounts.SingleOrDefault(e => e.Key.ToLower() == lev.ToLower()).Value,
                            IsChecked = false,
                            Value = lev
                        })
                });
            }

            if (filterGroupsStats.LanguageCounts.Any())
            {
                result.Add(new CheckboxFilter
                {
                    Title = "Languages",
                    RequestKey = "LanguageNames",
                    FilterOptions = filterGroups.LanguageNames!
                        .Where(l => filterGroupsStats.LanguageCounts.SingleOrDefault(e => e.Key.ToLower() == l.ToLower()).Value > 0)
                        .Select(lang => new FilterOption
                        {
                            Label = lang,
                            Count = filterGroupsStats.LanguageCounts.SingleOrDefault(e => e.Key.ToLower() == lang.ToLower()).Value,
                            IsChecked = false,
                            Value = lang
                        })
                        .OrderByDescending(x => x.Count)
                });
            }

            if (filterGroups.MinDuration != null && filterGroups.MaxDuration != null && (filterGroups.MinDuration != filterGroups.MaxDuration))
            {
                result.Add(new NumberRangeFilter
                {
                    Title = "Duration (Hours)",
                    RequestKey = "Duration",
                    MinRequestKey = "MinDuration",
                    MaxRequestKey = "MaxDuration",
                    MinValue = filterGroups.MinDuration ?? 0,
                    MaxValue = ((filterGroups.MaxDuration ?? 0) == (filterGroups.MinDuration ?? 1)) ? (filterGroups.MaxDuration ?? 0) + 1 : filterGroups.MaxDuration,
                    Step = 0.5d,
                    Unit = "H"
                });
            }

            if (filterGroups.MinEnrollments != null && filterGroups.MaxEnrollments != null && (filterGroups.MinEnrollments != filterGroups.MaxEnrollments))
            {
                result.Add(new NumberRangeFilter
                {
                    Title = "Enrollments",
                    RequestKey = "Enrollments",
                    MinRequestKey = "MinEnrollments",
                    MaxRequestKey = "MaxEnrollments",
                    MinValue = filterGroups.MinEnrollments ?? 0,
                    MaxValue = filterGroups.MaxEnrollments ?? 100,
                    Step = 1,
                    Unit = "STUDENTS"
                });
            }

            if (filterGroups.MinRating != null && filterGroups.MaxRating != null && (filterGroups.MinRating != filterGroups.MaxRating))
            {
                result.Add(new NumberRangeFilter
                {
                    Title = "Rating",
                    RequestKey = "Rating",
                    MinRequestKey = "MinRating",
                    MaxRequestKey = "MaxRating",
                    MinValue = filterGroups.MinRating ?? 0,
                    MaxValue = filterGroups.MaxRating ?? 5,
                    Step = 0.1d,
                    Unit = ""
                });
            }

            if (filterGroups.MinReviews != null && filterGroups.MaxReviews != null && (filterGroups.MinReviews != filterGroups.MaxReviews))
            {
                result.Add(new NumberRangeFilter
                {
                    Title = "Reviews",
                    RequestKey = "Reviews",
                    MinRequestKey = "MinReviews",
                    MaxRequestKey = "MaxReviews",
                    MinValue = filterGroups.MinReviews ?? 0,
                    MaxValue = filterGroups.MaxReviews ?? 10000,
                    Step = 1,
                    Unit = ""
                });
            }

            if (filterGroups.MinCreationDate != null && filterGroups.MaxCreationDate != null && (filterGroups.MinCreationDate != filterGroups.MaxCreationDate))
            {
                result.Add(new DateRangeFilter
                {
                    Title = "Creation Date",
                    RequestKey = "CreationDate",
                    MinRequestKey = "MinCreationDate",
                    MaxRequestKey = "MaxCreationDate",
                    MinDate = filterGroups.MinCreationDate ?? DateOnly.MinValue,
                    MaxDate = filterGroups.MaxCreationDate ?? DateOnly.FromDateTime(DateTime.Now)
                });
            }

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
}