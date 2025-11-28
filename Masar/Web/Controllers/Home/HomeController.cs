using BLL.DTOs.Misc;
using BLL.Helpers;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.ViewModels;
using Web.ViewModels.Home;
using Web.ViewModels.Misc;
using Web.ViewModels.Misc.FilterRequestVMs;


namespace Web.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseService _courseService;
        private readonly RazorViewToStringRenderer _razorRenderer;

        public HomeController(
            ILogger<HomeController> logger, 
            ICourseService courseService,
            RazorViewToStringRenderer razorRenderer
        ) {
            _logger = logger;
            _courseService = courseService;
            _razorRenderer = razorRenderer;
        }


        [HttpGet("~/")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("~/browse-courses")]
        public async Task<IActionResult> BrowseCourses()
        {
            var pagingRequest = new PagingRequestDto { CurrentPage = 1, PageSize = 30 };
            var initialRequest = new BrowseRequestDto();
            initialRequest.PagingRequest = pagingRequest;

            var coursesData = await _courseService.GetInitialBrowsePageCoursesAsync(pagingRequest);

            var viewModel = new BrowseCoursesViewModel
            {
                Data = new BrowseResultViewModel<CourseBrowseCardViewModel>
                {
                    Settings = new BrowseSettingsViewModel
                    {
                        FilterGroups = await BuildBrowseFilterGroupsAsync(),

                        PaginationSettings = new PaginationSettingsViewModel
                        {
                            CurrentPage = coursesData.Settings.PaginationSettings.CurrentPage,
                            TotalPages = coursesData.Settings.PaginationSettings.TotalPages,
                            PageSize = coursesData.Settings.PaginationSettings.PageSize,
                            TotalCount = coursesData.Settings.PaginationSettings.TotalCount
                        }
                    },

                    Items = coursesData.Items.Select(c => new CourseBrowseCardViewModel
                    {
                        CourseId = c.CourseId,
                        InstructorName = c.InstructorName,

                        Title = c.Title,
                        Description = c.Description,
                        ThumbnailImageUrl = c.ThumbnailImageUrl,
                        CreatedDate = c.CreatedDate.ToString("dd-MM-yyyy"),
                        MainCategory = c.MainCategory.Name,
                        Categories = c.Categories.Select(cat => cat.Name),
                        Languages = c.Languages.Select(lang => lang.Name),
                        Level = c.Level.ToString(),

                        // calculated fields
                        AverageRating = c.AverageRating,
                        NumberOfRatings = c.NumberOfRatings,

                        NumberOfStudents = c.NumberOfStudents,
                        NumberOfLectures = c.NumberOfLectures,
                        NumberOfMinutes = c.NumberOfMinutes
                    })
                },

                // UI specific properties
                PageTitle = "Masar | Every Code is a Step Forward",
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost("~/filter-courses")]
        public async Task<IActionResult> FilterCoursesPartial([FromBody] BrowseRequestDto request)
        {
            var browseResultDto = await _courseService.GetAllCoursesFilteredForBrowsingPagedAsync(request);

            var coursesGridBrowseVM = browseResultDto
                .Items
                .Select(c => new CourseBrowseCardViewModel
                {
                    CourseId = c.CourseId,
                    InstructorName = c.InstructorName,

                    Title = c.Title,
                    Description = c.Description,
                    ThumbnailImageUrl = c.ThumbnailImageUrl,
                    CreatedDate = c.CreatedDate.ToString("dd-MM-yyyy"),
                    MainCategory = c.MainCategory.Name,
                    Categories = c.Categories.Select(cat => cat.Name),
                    Languages = c.Languages.Select(lang => lang.Name),
                    Level = c.Level.ToString(),

                    // calculated fields
                    AverageRating = c.AverageRating,
                    NumberOfRatings = c.NumberOfRatings,

                    NumberOfStudents = c.NumberOfStudents,
                    NumberOfLectures = c.NumberOfLectures,
                    NumberOfMinutes = c.NumberOfMinutes
                });

            var paginationSettingsVM = new PaginationSettingsViewModel
            {
                CurrentPage = browseResultDto.Settings.PaginationSettings.CurrentPage,
                PageSize = browseResultDto.Settings.PaginationSettings.PageSize,
                TotalPages = browseResultDto.Settings.PaginationSettings.TotalPages,
                TotalCount = browseResultDto.Settings.PaginationSettings.TotalCount
            };


            var CoursesGrid = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_CoursesGridBrowsePartialView", coursesGridBrowseVM);
            var Pagination = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_PaginationPartialView", paginationSettingsVM);


            return Json(new
            {
                CoursesGrid = CoursesGrid,
                Pagination = Pagination,
                TotalCount = paginationSettingsVM.TotalCount
            });
        }

        private async Task<List<FilterGroupViewModel>> BuildBrowseFilterGroupsAsync()
        {
            var filterGroups = await _courseService.GetFilterSectionConfig();
            var filterGroupsStats = await _courseService.GetFilterGroupsStats();

            var result = new List<FilterGroupViewModel>();

            result.Add(new CheckboxFilter
            {
                Title = "Categories",
                RequestKey = "CategoryNames",

                FilterOptions = filterGroups.CategoryNames!.Select(cat => new FilterOption
                {
                    Label = cat,
                    Count = filterGroupsStats.CategoryCounts.SingleOrDefault(e => e.Key.ToLower() == cat.ToLower()).Value,
                    IsChecked = false,
                    Value = cat
                })
                .OrderByDescending(x => x.Count)
            });

            result.Add(new CheckboxFilter
            {
                Title = "Levels",
                RequestKey = "LevelNames",

                FilterOptions = filterGroups.LevelNames!.Select(lev => new FilterOption
                {
                    Label = lev,
                    Count = filterGroupsStats.LevelCounts.SingleOrDefault(e => e.Key.ToLower() == lev.ToLower()).Value,
                    IsChecked = false,
                    Value = lev
                })
                .OrderByDescending(x => x.Count)
            });

            result.Add(new CheckboxFilter
            {
                Title = "Languages",
                RequestKey = "LanguageNames",

                FilterOptions = filterGroups.LanguageNames!.Select(lang => new FilterOption
                {
                    Label = lang,
                    Count = filterGroupsStats.LanguageCounts.SingleOrDefault(e => e.Key.ToLower() == lang.ToLower()).Value,
                    IsChecked = false,
                    Value = lang
                })
                .OrderByDescending(x => x.Count)
            });


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


            result.Add(new DateRangeFilter
            {
                Title = "Creation Date",
                RequestKey = "CreationDate",

                MinRequestKey = "MinCreationDate",
                MaxRequestKey = "MaxCreationDate",
                MinDate = filterGroups.MinCreationDate ?? DateOnly.MinValue,
                MaxDate = filterGroups.MaxCreationDate ?? DateOnly.FromDateTime(DateTime.Now),
            });

            return result;
        }
    }
}