using BLL.DTOs.Misc;
using BLL.Helpers;
using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.ViewModels;
using DAL.Data;
using Web.Interfaces;
using Web.ViewModels.Home;
using Web.ViewModels.Misc;
using Web.ViewModels.Misc.FilterRequestVMs;


namespace Web.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICourseService _courseService;
        private readonly RazorViewToStringRenderer _razorRenderer;

        public HomeController(
            ILogger<HomeController> logger, 
            ICourseService courseService,
            RazorViewToStringRenderer razorRenderer,
            AppDbContext context,
            ICurrentUserService currentUserService
        ) {
            _logger = logger;
            _courseService = courseService;
            _razorRenderer = razorRenderer;
            _context = context;
            _currentUserService = currentUserService;
        }


        [HttpGet("~/")]
        public IActionResult Index()
        {
            var userId = _currentUserService.GetUserId();
            ViewBag.UserId = userId;

            return View();
        }


        [HttpGet("~/browse-courses")]
        public async Task<IActionResult> BrowseCourses()
        {
            var pagingRequest = new PagingRequestDto { CurrentPage = 1, PageSize = 3 };
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
                        NumberOfReviews = c.NumberOfReviews,

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
                    NumberOfReviews = c.NumberOfReviews,

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



        [HttpGet("~/course/{courseId:int}")]
        public async Task<IActionResult> CourseDetails(int courseId)
        {
            var courseDetails = await _courseService.GetCourseDetailsByIdAsync(courseId);
            
            if (courseDetails == null)
            {
                return NotFound("Course not found");
            }

            var userId = _currentUserService.GetUserId();
            
            // TODO: Check if student is enrolled
            bool isEnrolled = false;
            int? enrollmentId = null;
            decimal? progress = null;
            
            if (userId > 0)
            {
                // Check enrollment status
                // var enrollment = await _enrollmentService.GetStudentEnrollmentAsync(userId, courseId);
                // isEnrolled = enrollment != null;
                // enrollmentId = enrollment?.EnrollmentId;
                // progress = enrollment?.ProgressPercentage;
            }

            var viewModel = new CourseDetailsViewModel
            {
                Course = courseDetails,
                PageTitle = courseDetails.Title,
                IsEnrolled = isEnrolled,
                StudentEnrollmentId = enrollmentId,
                ProgressPercentage = progress
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private async Task<List<FilterGroupViewModel>> BuildBrowseFilterGroupsAsync()
        {
            var filterGroups = await _courseService.GetFilterSectionConfig();
            var filterGroupsStats = await _courseService.GetFilterGroupsStats();

            var result = new List<FilterGroupViewModel>();

            if (filterGroupsStats.CategoryCounts.Any())
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


            if (filterGroupsStats.LevelCounts.Any())
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
                });


            if (filterGroupsStats.LanguageCounts.Any())
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


            result.Add(new DateRangeFilter
            {
                Title = "Creation Date",
                RequestKey = "CreationDate",

                MinRequestKey = "MinCreationDate",
                MaxRequestKey = "MaxCreationDate",
                MinDate = filterGroups.MinCreationDate ?? DateOnly.MinValue,
                MaxDate = filterGroups.MaxCreationDate ?? DateOnly.FromDateTime(DateTime.Now)
            });


            //result.Add(new ToggleFilter
            //{
            //    Title = "Certified",
            //    RequestKey = "HasCertificates",

            //    Label = "Include Certificates",
            //    IsOn = filterGroups.HasCertificate ?? false
            //});

            return result;
        }
    }
}