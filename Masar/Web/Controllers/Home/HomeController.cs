using AutoMapper;
using BLL.DTOs.Misc;
using BLL.Helpers;
using BLL.Interfaces;
using BLL.Interfaces.Enrollment;
using Core.Entities;
using DAL.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Interfaces;
using Web.ViewModels.Home;
using Web.ViewModels.Misc;
using Web.ViewModels.Misc.FilterRequestVMs;

namespace Web.Controllers.Home;

public class HomeController(
    ILogger<HomeController> _logger,
    ICourseService _courseService,
    RazorViewToStringRenderer _razorRenderer,
    AppDbContext _context,
    ICurrentUserService _currentUserService,
    IEnrollmentService _enrollmentService,
    IMapper _mapper) : Controller
{
    [HttpGet("~/")]
    public async Task<IActionResult> Index()
    {
        var userId = _currentUserService.GetUserId();
        ViewBag.UserId = userId;

        var tracks = await _context.Tracks
            .Include(t => t.TrackCourses).ThenInclude(tc => tc.Course).ThenInclude(c => c!.Modules).ThenInclude(m => m.Lessons).ThenInclude(l => l.LessonContent)
            .Include(t => t.Enrollments)
            .Include(t => t.Categories).Take(6).ToListAsync();

        var viewModel = new HomeIndexViewModel
        {
            TotalStudents = await _context.Users.CountAsync(),
            TotalCourses = await _context.Courses.CountAsync(),
            TotalInstructors = await _context.InstructorProfiles.CountAsync(),
            FeaturedTracks = _mapper.Map<List<HomeFeaturedTrackViewModel>>(tracks)
        };

        return View(viewModel);
    }

    [HttpGet("~/browse-courses")]
    public async Task<IActionResult> BrowseCourses()
    {
        var pagingRequest = new PagingRequestDto { CurrentPage = 1, PageSize = 6 };
        var coursesData = await _courseService.GetInitialBrowsePageCoursesAsync(pagingRequest);

        var viewModel = new BrowseCoursesViewModel
        {
            Data = new BrowseResultViewModel<CourseBrowseCardViewModel>
            {
                Settings = new BrowseSettingsViewModel
                {
                    FilterGroups = await BuildBrowseFilterGroupsAsync(),
                    PaginationSettings = _mapper.Map<PaginationSettingsViewModel>(coursesData.Settings.PaginationSettings)
                },
                Items = _mapper.Map<IEnumerable<CourseBrowseCardViewModel>>(coursesData.Items)
            },
            PageTitle = "Masar | Every Code is a Step Forward",
        };

        return View(viewModel);
    }

    [HttpPost("~/filter-courses")]
    public async Task<IActionResult> FilterCoursesPartial([FromBody] BrowseRequestDto request)
    {
        var browseResultDto = await _courseService.GetAllCoursesFilteredForBrowsingPagedAsync(request);

        var coursesGridBrowseVM = _mapper.Map<IEnumerable<CourseBrowseCardViewModel>>(browseResultDto.Items);
        var paginationSettingsVM = _mapper.Map<PaginationSettingsViewModel>(browseResultDto.Settings.PaginationSettings);

        var CoursesGrid = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_CoursesGridBrowsePartialView", coursesGridBrowseVM);
        var Pagination = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_PaginationPartialView", paginationSettingsVM);

        return Json(new { CoursesGrid, Pagination, TotalCount = paginationSettingsVM.TotalCount });
    }

    [HttpGet("~/browse-tracks")]
    public async Task<IActionResult> BrowseTracks(int page = 1, int pageSize = 6, string sortBy = "popularity", string sortOrder = "desc")
    {
        var tracksQuery = _context.Tracks
            .Include(t => t.TrackCourses).ThenInclude(tc => tc.Course).ThenInclude(c => c!.Modules).ThenInclude(m => m.Lessons).ThenInclude(l => l.LessonContent)
            .Include(t => t.Enrollments)
            .Include(t => t.Categories).AsQueryable();

        var totalCount = await tracksQuery.CountAsync();

        tracksQuery = sortBy?.ToLower() switch
        {
            "title" => sortOrder?.ToLower() == "asc" ? tracksQuery.OrderBy(t => t.Title) : tracksQuery.OrderByDescending(t => t.Title),
            "rating" => sortOrder?.ToLower() == "asc" ? tracksQuery.OrderBy(t => t.Id) : tracksQuery.OrderByDescending(t => t.Id),
            "duration" => sortOrder?.ToLower() == "asc" ? tracksQuery.OrderBy(t => t.TrackCourses.Count) : tracksQuery.OrderByDescending(t => t.TrackCourses.Count),
            _ => sortOrder?.ToLower() == "asc" ? tracksQuery.OrderBy(t => t.Enrollments.Count) : tracksQuery.OrderByDescending(t => t.Enrollments.Count)
        };

        var tracks = await tracksQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var viewModel = new HomeBrowseTracksViewModel
        {
            PageTitle = "Browse Learning Tracks | Masar",
            TotalCount = totalCount,
            PaginationSettings = new PaginationSettingsViewModel { CurrentPage = page, PageSize = pageSize, TotalPages = totalPages, TotalCount = totalCount },
            Tracks = _mapper.Map<List<HomeTrackCardViewModel>>(tracks)
        };

        return View(viewModel);
    }

    [HttpPost("~/filter-tracks")]
    public async Task<IActionResult> FilterTracksPartial([FromBody] BrowseTracksRequestDto request)
    {
        var tracksQuery = _context.Tracks
            .Include(t => t.TrackCourses).ThenInclude(tc => tc.Course).ThenInclude(c => c!.Modules).ThenInclude(m => m.Lessons).ThenInclude(l => l.LessonContent)
            .Include(t => t.Enrollments)
            .Include(t => t.Categories).AsQueryable();

        var totalCount = await tracksQuery.CountAsync();

        tracksQuery = request.SortBy?.ToLower() switch
        {
            "title" => request.SortOrder?.ToLower() == "asc" ? tracksQuery.OrderBy(t => t.Title) : tracksQuery.OrderByDescending(t => t.Title),
            "rating" => request.SortOrder?.ToLower() == "asc" ? tracksQuery.OrderBy(t => t.Id) : tracksQuery.OrderByDescending(t => t.Id),
            "duration" => request.SortOrder?.ToLower() == "asc" ? tracksQuery.OrderBy(t => t.TrackCourses.Count) : tracksQuery.OrderByDescending(t => t.TrackCourses.Count),
            _ => request.SortOrder?.ToLower() == "asc" ? tracksQuery.OrderBy(t => t.Enrollments.Count) : tracksQuery.OrderByDescending(t => t.Enrollments.Count)
        };

        var tracks = await tracksQuery.Skip((request.CurrentPage - 1) * request.PageSize).Take(request.PageSize).ToListAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        var trackCards = _mapper.Map<List<HomeTrackCardViewModel>>(tracks);
        var paginationSettings = new PaginationSettingsViewModel { CurrentPage = request.CurrentPage, PageSize = request.PageSize, TotalPages = totalPages, TotalCount = totalCount };

        var TracksGrid = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_TracksGridBrowsePartialView", trackCards);
        var Pagination = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_PaginationPartialView", paginationSettings);

        return Json(new { TracksGrid, Pagination, TotalCount = totalCount });
    }

    [HttpGet("~/track/{trackId:int}")]
    public async Task<IActionResult> TrackDetails(int trackId)
    {
        var track = await _context.Tracks
            .Include(t => t.TrackCourses).ThenInclude(tc => tc.Course).ThenInclude(c => c!.Modules).ThenInclude(m => m.Lessons).ThenInclude(l => l.LessonContent)
            .Include(t => t.TrackCourses).ThenInclude(tc => tc.Course).ThenInclude(c => c!.Instructor).ThenInclude(i => i!.User)
            .Include(t => t.Categories)
            .Include(t => t.Enrollments)
            .FirstOrDefaultAsync(t => t.Id == trackId);

        if (track == null) return NotFound("Track not found");

        var userId = _currentUserService.GetUserId();
        bool isEnrolled = false;

        if (userId > 0)
        {
            var studentProfile = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
            if (studentProfile != null) isEnrolled = await _enrollmentService.IsStudentEnrolledInTrackAsync(studentProfile.StudentId, trackId);
        }

        var courses = track.TrackCourses?.Select(tc => tc.Course).Where(c => c != null).Select(c => new TrackCourseDetailViewModel
        {
            CourseId = c!.Id,
            Title = c.Title,
            Description = c.Description ?? "",
            ThumbnailImageUrl = c.ThumbnailImageUrl,
            Level = c.Level.ToString(),
            InstructorName = c.Instructor?.User != null ? $"{c.Instructor.User.FirstName} {c.Instructor.User.LastName}" : "Instructor",
            DurationHours = CalculateCourseDuration(c),
            ModulesCount = c.Modules?.Count ?? 0,
            LessonsCount = c.Modules?.Sum(m => m.Lessons?.Count ?? 0) ?? 0,
            Rating = 4.7m
        }).ToList() ?? new List<TrackCourseDetailViewModel>();

        var viewModel = new TrackDetailsViewModel
        {
            TrackId = track.Id,
            Title = track.Title,
            Description = track.Description ?? "Start your learning journey",
            CategoryName = track.Categories?.FirstOrDefault()?.Name ?? "Learning Track",
            CategoryIcon = GetCategoryIcon(track.Categories?.FirstOrDefault()?.Name),
            Level = "Beginner",
            DurationHours = CalculateTrackDuration(track),
            CoursesCount = courses.Count,
            StudentsCount = track.Enrollments?.Count ?? 0,
            Rating = 4.8m,
            Skills = ExtractSkills(track),
            Courses = courses,
            IsEnrolled = isEnrolled,
            PageTitle = track.Title
        };

        return View(viewModel);
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
                FilterOptions = filterGroups.CategoryNames!.Where(c => filterGroupsStats.CategoryCounts.SingleOrDefault(e => e.Key.ToLower() == c.ToLower()).Value > 0).Select(cat => new FilterOption
                {
                    Label = cat,
                    Count = filterGroupsStats.CategoryCounts.SingleOrDefault(e => e.Key.ToLower() == cat.ToLower()).Value,
                    IsChecked = false,
                    Value = cat
                }).OrderByDescending(x => x.Count)
            });
        }

        if (filterGroupsStats.LevelCounts.Any())
        {
            result.Add(new CheckboxFilter
            {
                Title = "Levels",
                RequestKey = "LevelNames",
                FilterOptions = filterGroups.LevelNames!.Where(l => filterGroupsStats.LevelCounts.SingleOrDefault(e => e.Key.ToLower() == l.ToLower()).Value > 0).Select(lev => new FilterOption
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
                FilterOptions = filterGroups.LanguageNames!.Where(l => filterGroupsStats.LanguageCounts.SingleOrDefault(e => e.Key.ToLower() == l.ToLower()).Value > 0).Select(lang => new FilterOption
                {
                    Label = lang,
                    Count = filterGroupsStats.LanguageCounts.SingleOrDefault(e => e.Key.ToLower() == lang.ToLower()).Value,
                    IsChecked = false,
                    Value = lang
                }).OrderByDescending(x => x.Count)
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

    private int CalculateTrackDuration(Track track)
    {
        if (track.TrackCourses == null || !track.TrackCourses.Any()) return 0;
        var totalSeconds = track.TrackCourses.Where(tc => tc.Course?.Modules != null).SelectMany(tc => tc.Course!.Modules!).SelectMany(m => m.Lessons ?? new List<Lesson>()).Select(l => l.LessonContent).OfType<VideoContent>().Sum(v => v.DurationInSeconds);
        return (int)Math.Ceiling(totalSeconds / 3600.0);
    }

    private int CalculateCourseDuration(Course course)
    {
        if (course.Modules == null || !course.Modules.Any()) return 0;
        var totalSeconds = course.Modules.SelectMany(m => m.Lessons ?? new List<Lesson>()).Select(l => l.LessonContent).OfType<VideoContent>().Sum(v => v.DurationInSeconds);
        return (int)Math.Ceiling(totalSeconds / 3600.0);
    }

    private List<string> ExtractSkills(Track track)
    {
        return track.TrackCourses?.SelectMany(tc => tc.Course?.Categories ?? new List<Category>()).Select(c => c.Name).Distinct().Take(4).ToList() ?? new List<string>();
    }

    private string GetCategoryIcon(string? categoryName)
    {
        return categoryName?.ToLower() switch
        {
            "web development" => "fa-laptop-code",
            "data science" => "fa-brain",
            "mobile development" => "fa-mobile-alt",
            "design" => "fa-palette",
            "cloud computing" => "fa-cloud",
            "devops" => "fa-server",
            _ => "fa-book"
        };
    }
}