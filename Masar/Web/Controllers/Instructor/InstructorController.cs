using BLL.DTOs.Admin;
using BLL.DTOs.Instructor;
using BLL.DTOs.Instructor.CreateCourse;
using BLL.DTOs.Instructor.ManageCourse;
using BLL.DTOs.Misc;
using BLL.Helpers;
using BLL.Interfaces.Instructor;
using Core.Entities;
using Core.Entities.Enums;
using Core.RepositoryInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web.Interfaces;
using Web.ViewModels.Instructor;
using Web.ViewModels.Instructor.CreateCourse;
using Web.ViewModels.Instructor.Dashboard;
using Web.ViewModels.Instructor.ManageCourse;
using Web.ViewModels.Misc;
using DAL.Data;

namespace Web.Controllers.Instructor;

[Authorize(Roles = "Instructor")]
public class InstructorController : Controller
{
    private readonly IInstructorDashboardService _dashboardService;
    private readonly IInstructorCoursesService _coursesService;
    private readonly IInstructorProfileService _profileService;
    private readonly RazorViewToStringRenderer _razorRenderer;
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IInstructorProfileService _instructorProfileService;
    private readonly IInstructorManageCourseService _instructorManageCourseService;
    private readonly AppDbContext _context;


    public InstructorController(
        IInstructorDashboardService dashboardService, 
        IInstructorCoursesService courseService,
        IInstructorProfileService profileService,
        RazorViewToStringRenderer razorRenderer,
        UserManager<User> userManager,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ICategoryRepository categoryRepository,
        IInstructorProfileService instructorProfileService,
        IInstructorManageCourseService instructorManageCourseService,
        AppDbContext context)
    {
        _dashboardService = dashboardService;
        _coursesService = courseService;
        _profileService = profileService;
        _razorRenderer = razorRenderer;
        _userManager = userManager;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _categoryRepository = categoryRepository;
        _instructorProfileService = instructorProfileService;
        _instructorManageCourseService = instructorManageCourseService;
        _context = context;
    }

    private async Task<int> GetInstructorIdAsync()
    {
        var userId = _currentUserService.GetUserId();
        
        if (userId == 0)
            return 0;
        
        var instructorProfile = await _userRepository.GetInstructorProfileForUserAsync(userId, includeUserBase: false);
        
        return instructorProfile?.InstructorId ?? 0;
    }

    private async Task SetInstructorViewBagDataAsync()
    {
        var instructorId = await GetInstructorIdAsync();

        if (instructorId > 0)
        {
            var instructorProfile = await _userRepository.GetInstructorProfileAsync(instructorId, includeUserBase: true);
            if (instructorProfile?.User != null)
            {
                ViewBag.InstructorName = $"{instructorProfile.User.FirstName} {instructorProfile.User.LastName}";
            }
        }
    }

    /// <summary>
    /// Instructor Dashboard - Overview of performance, stats, and quick actions.
    /// </summary>
    [HttpGet("/instructor/dashboard")]
    [HttpGet("/instructor")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Title = "Instructor Dashboard | Masar";
        await SetInstructorViewBagDataAsync();

        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized("Instructor profile not found");

        var dashboardData = await _dashboardService.GetInstructorDashboardAsync(instructorId);

        var viewModel = new InstructorDashboardViewModel
        {
            Data = new InstructorDashboardDataViewModel
            {
                InstructorId = dashboardData.InstructorId,
                InstructorName = dashboardData.InstructorName,

                GeneralStats = new InstructorGeneralStatsViewModel
                {
                    TotalCourses = dashboardData.GeneralStats.TotalCourses,
                    TotalStudents = dashboardData.GeneralStats.TotalStudents,
                    CompletionRate = dashboardData.GeneralStats.CompletionRate,
                    AverageRating = dashboardData.GeneralStats.AverageRating
                },

                CurrentMonthStats = new InstructorCurrentMonthStatsViewModel
                {
                    NewStudents = dashboardData.CurrentMonthStats.NewStudents,
                    NewReviews = dashboardData.CurrentMonthStats.NewReviews,
                    Completions = dashboardData.CurrentMonthStats.NewCompletions
                },

                TopPerformingCourses = dashboardData.TopPerformingCourses.Select(c => 
                    new InstructorTopPerformingCourseViewModel
                    {
                        Title = c.Title,
                        StudentsEnrolled = c.StudentsEnrolled,
                        AverageRating = c.AverageRating
                    }
                ),

                CourseCards = dashboardData.CourseCards.Select(c => 
                    new InstructorCourseCardViewModel
                    {
                        CourseId = c.CourseId,
                        Title = c.Title,
                        Description = c.Description,
                        
                        MainCategory = c.MainCategory,
                        Level = c.Level.ToString(),

                        ModulesCount = c.ModulesCount,
                        StudentsCount = c.StudentsCount,
                        DurationHours = c.DurationHours,
                        
                        Rating = c.Rating,
                        Status = c.Status
                    }
                )
            },


            // UI specific properties
            PageTitle = "Instructor Overview",
            // Dynamic greeting message based on time of day
            GreetingMessage = DateTime.Now.Hour < 12 ? "Good Morning" : "Good Evening",
        };

        return View(viewModel);
    }

    /// <summary>
    /// My Courses - Manage and view courses you are teaching.
    /// </summary>
    [HttpGet("/instructor/my-courses")]
    public async Task<IActionResult> MyCourses()
    {
        ViewBag.Title = "My Courses | Masar";
        await SetInstructorViewBagDataAsync();
        
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized("Instructor profile not found");
            
        var initialRequest = new PagingRequestDto() { CurrentPage = 1, PageSize = 6 };
        var coursesData = await _coursesService.GetInstructorCoursesPagedAsync(instructorId, initialRequest);
        
        var viewModel = new InstructorCoursesViewModel
        {
            Data = new PagedResultViewModel<InstructorCourseViewModel>
            {
                Settings = new PaginationSettingsViewModel
                {
                    CurrentPage = coursesData.Settings.CurrentPage,
                    PageSize = coursesData.Settings.PageSize,
                    TotalPages = coursesData.Settings.TotalPages,
                    TotalCount = coursesData.Settings.TotalCount,
                },

                Items = coursesData.Items.Select(c => new InstructorCourseViewModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    ThumbnailImageUrl = c.ThumbnailImageUrl,
                    Status = c.Status,

                    CreatedDate = c.CreatedDate?.ToString("dd-MM-yyyy") ?? "",
                    LastUpdatedDate = c.LastUpdatedDate?.ToString("dd-MM-yyyy") ?? "",

                    Level = c.Level.ToString(),
                    MainCategory = c.MainCategory ?? "UnCategorized",

                    NumberOfStudents = c.NumberOfStudents,
                    NumberOfModules = c.NumberOfModules,
                    NumberOfHours = c.NumberOfMinutes / 60,
                    NumberOfMinutes = c.NumberOfMinutes % 60,
                    AverageRating = c.AverageRating
                })
            },

            // UI specific properties
            PageTitle = "My Courses | Masar",
        };

        return View(viewModel);
    }

    /// <summary>
    /// GetCoursesPartial - AJAX endpoint to retrieve paginated courses.
    /// </summary>
    [HttpPost("/instructor/my-courses")]
    public async Task<IActionResult> GetCoursesPartial([FromBody] PagingRequestDto request)
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized("Instructor profile not found");
            
        var coursesData = await _coursesService.GetInstructorCoursesPagedAsync(instructorId, request);
        
        var pagedResult = new PagedResultViewModel<InstructorCourseViewModel>
        {
            Settings = new PaginationSettingsViewModel
            {
                CurrentPage = coursesData.Settings.CurrentPage,
                PageSize = coursesData.Settings.PageSize,
                TotalPages = coursesData.Settings.TotalPages,
                TotalCount = coursesData.Settings.TotalCount,
            },

            Items = coursesData.Items.Select(c => new InstructorCourseViewModel
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                ThumbnailImageUrl = c.ThumbnailImageUrl,
                Status = c.Status,

                CreatedDate = c.CreatedDate?.ToString("MM-dd-yyyy") ?? "",
                LastUpdatedDate = c.LastUpdatedDate?.ToString("MM-dd-yyyy") ?? "",

                Level = c.Level.ToString(),
                MainCategory = c.MainCategory ?? "UnCategorized",

                NumberOfStudents = c.NumberOfStudents,
                NumberOfModules = c.NumberOfModules,
                NumberOfHours = c.NumberOfMinutes / 60,
                NumberOfMinutes = c.NumberOfMinutes % 60,
                AverageRating = c.AverageRating
            })
        };

        var CoursesGrid = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_CoursesGridPartialView", pagedResult.Items);
        var Pagination = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_PaginationPartialView", pagedResult.Settings);

        return Json(new
        {
            CoursesGrid = CoursesGrid,
            Pagination = Pagination
        });
    }

    /// <summary>
    /// Profile - View and edit your instructor profile.
    /// </summary>
    [HttpGet("/instructor/profile")]
    public async Task<IActionResult> Profile()
    {
        ViewBag.Title = "Instructor Profile | Masar";
        await SetInstructorViewBagDataAsync();

        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized("Instructor profile not found");
            
        var profileData = await _profileService.GetInstructorProfileAsync(instructorId);

        if (profileData == null)
        {
            return NotFound();
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

        var viewModel = new InstructorProfileViewModel
        {
            Data = new InstructorProfileDataViewModel
            {
                InstructorId = profileData.InstructorId,
                UserId = profileData.UserId,
                FirstName = profileData.FirstName,
                LastName = profileData.LastName,
                FullName = profileData.FullName,
                Username = $"@{profileData.FirstName.ToLower()}_{profileData.LastName.ToLower()}",
                Email = profileData.Email,
                Phone = profileData.Phone,
                ProfilePicture = profileData.ProfilePicture,
                Bio = profileData.Bio,
                Initials = $"{profileData.FirstName[0]}{profileData.LastName[0]}",

                YearsOfExperience = profileData.YearsOfExperience,
                JoinedDate = profileData.JoinedDate.ToString("MMMM yyyy"),

                GithubUrl = profileData.GithubUrl,
                LinkedInUrl = profileData.LinkedInUrl,
                FacebookUrl = profileData.FacebookUrl,
                WebsiteUrl = profileData.WebsiteUrl,

                Stats = new InstructorProfileStatsViewModel
                {
                    TotalCourses = profileData.TotalCourses,
                    TotalStudents = profileData.TotalStudents,
                    AverageRating = profileData.AverageRating,
                    TotalReviews = profileData.TotalReviews,
                    TeachingStreak = profileData.TeachingStreak,
                    TotalTeachingHours = profileData.TotalTeachingHours,
                    StudentInteractions = profileData.StudentInteractions,
                    CertificatesIssued = profileData.CertificatesIssued
                },

                Skills = profileData.Skills,

                Courses = profileData.Courses.Select((c, index) => new InstructorProfileCourseCardViewModel
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Status = c.Status,
                    StatusBadgeClass = c.Status.ToLower() == "published" ? "badge-green" : "badge-orange",
                    StudentsCount = c.StudentsCount,
                    Rating = c.Rating,
                    GradientStyle = gradients[index % gradients.Length]
                }).ToList()
            },

            PageTitle = "Instructor Profile"
        };

        return View(viewModel);
    }

    /// <summary>
    /// Get Edit Profile Form - Returns the edit profile partial view for the modal.
    /// </summary>
    [HttpGet("/instructor/profile/edit")]
    public async Task<IActionResult> GetEditProfileForm()
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized("Instructor profile not found");

        var profileData = await _profileService.GetInstructorProfileAsync(instructorId);

        if (profileData == null)
            return NotFound();

        var formModel = new EditProfileFormModel
        {
            FirstName = profileData.FirstName,
            LastName = profileData.LastName,
            Phone = profileData.Phone,
            YearsOfExperience = profileData.YearsOfExperience,
            Bio = profileData.Bio,
            Skills = profileData.Skills,
            GithubUrl = profileData.GithubUrl,
            LinkedInUrl = profileData.LinkedInUrl,
            FacebookUrl = profileData.FacebookUrl,
            WebsiteUrl = profileData.WebsiteUrl
        };

        return PartialView("_EditProfileModal", formModel);
    }

    /// <summary>
    /// Update Profile - Handle profile updates from the edit form.
    /// </summary>
    [HttpPost("/instructor/profile/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile([FromBody] EditProfileFormModel formModel)
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Json(new { success = false, message = "Instructor profile not found" });

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, message = "Validation failed", errors });
        }

        var updateDto = new UpdateInstructorProfileDto
        {
            FirstName = formModel.FirstName,
            LastName = formModel.LastName,
            Phone = formModel.Phone,
            YearsOfExperience = formModel.YearsOfExperience,
            Bio = formModel.Bio,
            Skills = formModel.Skills,
            GithubUrl = formModel.GithubUrl,
            LinkedInUrl = formModel.LinkedInUrl,
            FacebookUrl = formModel.FacebookUrl,
            WebsiteUrl = formModel.WebsiteUrl
        };

        var result = await _profileService.UpdateInstructorProfileAsync(instructorId, updateDto);

        if (result)
            return Json(new { success = true, message = "Profile updated successfully!" });
        else
            return Json(new { success = false, message = "Failed to update profile" });
    }

    /// <summary>
    /// Upload Image - Handle profile photo uploads for instructor.
    /// </summary>
    [HttpPost("/instructor/upload-image")]
    [IgnoreAntiforgeryToken] // Allow upload without token since we handle it manually
    public async Task<IActionResult> UploadImage(IFormFile file, string type)
    {
        Console.WriteLine($"Instructor UploadImage called - file: {file?.FileName}, type: {type}");
        
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return Json(new { success = false, message = "Invalid file type. Allowed: jpg, jpeg, png, webp, gif" });

        if (file.Length > 5 * 1024 * 1024)
            return Json(new { success = false, message = "File size exceeds 5MB" });

        try
        {
            var instructorId = await GetInstructorIdAsync();
            Console.WriteLine($"Instructor UploadImage - instructorId: {instructorId}");
            
            if (instructorId == 0)
                return Json(new { success = false, message = "Instructor profile not found" });

            var folder = type == "cover" ? "covers" : "profiles";
            var fileName = $"{folder}-instructor-{instructorId}-{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
            
            Console.WriteLine($"Instructor UploadImage - uploadsFolder: {uploadsFolder}");
            
            Directory.CreateDirectory(uploadsFolder);
            
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            var fileUrl = $"/uploads/{folder}/{fileName}";
            Console.WriteLine($"Instructor UploadImage - fileUrl: {fileUrl}");

            // If it's a profile picture, update the user's picture in database
            if (type == "profile")
            {
                var instructorProfile = await _userRepository.GetInstructorProfileAsync(instructorId, includeUserBase: true);
                if (instructorProfile?.User != null)
                {
                    instructorProfile.User.Picture = fileUrl;
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Instructor UploadImage - saved picture to database");
                }
            }

            return Json(new { success = true, url = fileUrl });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Instructor UploadImage error: {ex.Message}");
            Console.WriteLine($"Instructor UploadImage stack: {ex.StackTrace}");
            return Json(new { success = false, message = "Upload failed: " + ex.Message });
        }
    }




    [HttpGet("/instructor/create-course")]
    public async Task<IActionResult> CreateCourse()
    {
        var categories = await _categoryRepository.GetAllQueryable()
            .Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            })
            .ToListAsync();

        var viewModel = new CreateCourseViewModel
        {
            CategoryOptions = categories
        };

        return View(viewModel);
    }

    [HttpPost("/instructor/create-course")]
    public async Task<IActionResult> CreateCourse(CreateCourseViewModel model)
    {
        // is instructor logged in and authenticated
        var instructorId = _currentUserService.GetInstructorId();
        if (!await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Unauthorized();

        // 1. Validation
        if (!ModelState.IsValid)
        {
            // RE-POPULATE the dropdown if validation fails!
            // The View does not "remember" the list between requests.
            model.CategoryOptions = await _categoryRepository.GetAllQueryable()
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name,
                })
                .ToListAsync();

            return View(model);
        }

        // 2. Success Logic (Creating the course row int the database)
        var dto = new CreateCourseBasicDetailsDto
        {
            CourseTitle = model.CourseTitle,
            MainCategoryId = model.MainCategoryId ?? 0,
        };

        var newCourseId = await _coursesService.CreateCourseAsync(instructorId, dto);
        if (newCourseId == null)
            return Unauthorized();

        // 3. Redirect
        return RedirectToAction("ManageCourse", new { courseId = newCourseId });
    }


    [HttpGet("/instructor/manage-course/{courseId:int:required}")]
    public async Task<IActionResult> ManageCourse(int courseId)
    {
        if (!ModelState.IsValid)
            return RedirectToAction("MyCourses");

        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Unauthorized("Instructor profile not found");

        var haveCourse = await _instructorProfileService.HasCourseWithIdAsync(instructorId, courseId);
        if (!haveCourse)
            return NotFound("Requested course doesn't exist or doesn't belong to this instructor");

        // validated both instructor and course (both exist and course is owned by this instructor)
        var data = await _instructorManageCourseService.GetCourseForManageAsync(instructorId, courseId);
        if (data == null)
            return NotFound("Can't load course data");

        // Fetch additional course data for BasicInfo and LearningOutcomes tabs
        var course = await _context.Courses
            .Include(c => c.Categories)
            .Include(c => c.Languages)
            .Include(c => c.LearningOutcomes)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        // Get all categories and languages for dropdowns
        var allCategories = await _categoryRepository.GetAllAsync();
        var allLanguages = await _context.Languages.ToListAsync();

        var viewModel = new ManageCourseViewModel
        {
            CourseId = data.CourseId,
            CourseTitle = data.CourseTitle,
            CourseStatus = data.CourseStatus.ToString(),

            // Curriculum Tab
            Curriculum = new ManageCourseCurriculumViewModel
            {
                CourseId = data.CourseId,
                CourseTitle = data.CourseTitle,
                CourseStatus = data.CourseStatus.ToString(),

                Modules = data.Curriculum?.Modules.Select(m => new ManageViewModuleViewModel
                {
                    ModuleId = m.ModuleId,
                    ModuleTitle = m.ModuleTitle,
                    ModuleOrder = m.ModuleOrder,
                    LessonsCount = m.LessonsCount,
                    FormattedLessonDuration = FormatDuration(m.DurationInMinutes ?? 0),

                    Lessons = m.Lessons.Select(l => new ManageViewLessonViewModel
                    {
                        LessonId = l.LessonId,
                        LessonTitle = l.LessonTitle,
                        LessonOrder = l.LessonOrder,
                        ContentType = l.ContentType.ToString(),
                        FormattedLessonDuration = FormatDuration(l.DurationInMinutes ?? 0)
                    }),
                }) ?? [],

                CurriculumStats = new ManageCourseCurriculumStatsViewModel
                {
                    TotalModules = data.Curriculum?.Modules.Count() ?? 0,

                    VideoLessonsCount = data.Curriculum?.Modules
                        .SelectMany(m => m.Lessons)
                        .Count(l => l.ContentType == LessonContentType.Video) ?? 0,

                    ArticleLessonsCount = data.Curriculum?.Modules
                        .SelectMany(m => m.Lessons)
                        .Count(l => l.ContentType == LessonContentType.Article) ?? 0,

                    FormattedCourseDuration = FormatDuration(data.Curriculum?.Modules
                        .Where(m => m.DurationInMinutes != null)
                        .Sum(m => m.DurationInMinutes) ?? 0)
                }
            },

            // Basic Info Tab
            BasicInfo = new ManageCourseBasicInfoViewModel
            {
                CourseId = courseId,
                CourseStatus = data.CourseStatus.ToString(),
                CourseTitle = course?.Title ?? data.CourseTitle,
                CourseDescription = course?.Description ?? "",
                ThumbnailImageUrl = course?.ThumbnailImageUrl,
                Level = course?.Level ?? CourseLevel.Beginner,

                Categories = course?.Categories?.Select(c => new ManageCourseCategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.Name
                }).ToList() ?? [],

                Languages = course?.Languages?.Select(l => new ManageCourseLanguageViewModel
                {
                    LanguageId = l.LanguageId,
                    LanguageName = l.Name
                }).ToList() ?? [],

                AllCategories = allCategories.Select(c => new ManageCourseCategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.Name
                }).ToList(),

                AllLanguages = allLanguages.Select(l => new ManageCourseLanguageViewModel
                {
                    LanguageId = l.LanguageId,
                    LanguageName = l.Name
                }).ToList(),

                AllLevels = Enum.GetValues<CourseLevel>().ToList()
            },

            // Learning Outcomes Tab
            LearningOutcomes = new ManageCourseLearningOutcomesViewModel
            {
                CourseId = courseId,
                CourseTitle = data.CourseTitle,
                CourseStatus = data.CourseStatus.ToString(),

                LearningOutcomes = course?.LearningOutcomes?.Select(lo => new CourseLearningOutcomeViewModel
                {
                    LearningOutcomeId = lo.Id,
                    OutcomeName = lo.Title ?? lo.Description ?? ""
                }).ToList() ?? []
            }
        };

        return View(viewModel);
    }

    [HttpGet("/instructor/manage-course/{courseId:int:required}/curriculum")]
    public async Task<IActionResult> GetCourseCurriculumPartialAsync(int courseId)
    {
        if (!ModelState.IsValid)
            return NotFound("Requseted course is not found");

        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Unauthorized("Instructor profile not found");

        var haveCourse = await _instructorProfileService.HasCourseWithIdAsync(instructorId, courseId);
        if (!haveCourse)
            return NotFound("Requested course doesn't exist or doesn't belong to this instructor");

        // validated both instructor and course (both exist and course is owned by this instructor)
        var curriculumDataDto = await _instructorManageCourseService.GetCourseCurriculumAsync(instructorId, courseId);
        if (curriculumDataDto == null)
            return NotFound("Course curriculum couldn't be loaded");


        var curriculumViewModel = new ManageCourseCurriculumViewModel
        {
            CourseId = curriculumDataDto.CourseId,
            CourseTitle = curriculumDataDto.CourseTitle,
            CourseStatus = curriculumDataDto.CourseStatus.ToString(),

            Modules = curriculumDataDto.Modules.Select(m => new ManageViewModuleViewModel
            {
                ModuleId = m.ModuleId,
                ModuleTitle = m.ModuleTitle,
                ModuleOrder = m.ModuleOrder,
                LessonsCount = m.LessonsCount,
                FormattedLessonDuration = FormatDuration(m.DurationInMinutes ?? 0),

                Lessons = m.Lessons.Select(l => new ManageViewLessonViewModel
                {
                    LessonId = l.LessonId,
                    LessonTitle = l.LessonTitle,
                    LessonOrder = l.LessonOrder,
                    ContentType = l.ContentType.ToString(),

                    FormattedLessonDuration = FormatDuration(l.DurationInMinutes ?? 0)
                }),
            }),

            CurriculumStats = new ManageCourseCurriculumStatsViewModel
            {
                TotalModules = curriculumDataDto.Modules.Count(),

                VideoLessonsCount = curriculumDataDto.Modules
                        .SelectMany(m => m.Lessons)
                        .Count(l => l.ContentType == LessonContentType.Video),

                ArticleLessonsCount = curriculumDataDto.Modules
                        .SelectMany(m => m.Lessons)
                        .Count(l => l.ContentType == LessonContentType.Article),

                FormattedCourseDuration = FormatDuration(curriculumDataDto.Modules
                        .Where(m => m.DurationInMinutes != null)
                        .Sum(m => m.DurationInMinutes) ?? 0
                )
            }
        };

        var CoursesGrid = await _razorRenderer.RenderViewToStringAsync(ControllerContext, "_ManageCourseCurriculumPartialView", curriculumViewModel);

        return Json(new
        {
            CoursesGrid = CoursesGrid,

            CourseId = curriculumDataDto.CourseId,
            CourseTitle = curriculumDataDto.CourseTitle,
            CourseStatus = curriculumDataDto.CourseStatus
        });
    }

    [HttpGet("/instructor/manage-course/lesson/{lessonId:int:required}")]
    public async Task<IActionResult> GetLessonDataJsonAsync(int lessonId)
    {
        if (!ModelState.IsValid)
            return NotFound("requested lesson is not found");

        // authenticate the user
        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Unauthorized("user is not an instructor");

        //authorization to access the course and its lessons
        var courseId = await _instructorProfileService.GetCourseIdForLessonAsync(instructorId, lessonId);
        if (courseId == null)
            return Forbid();

        // authenticated and authorized ==> fetch the data
        var lessonDataDto = await _instructorManageCourseService.GetLessonDataAsync(instructorId, lessonId);

        if (lessonDataDto == null)
            return NotFound("Requested lesson data couldn't be found");

        var viewModel = new ManageEditLessonViewModel
        {
            LessonId = lessonId,
            LessonTitle = lessonDataDto.LessonTitle,
            LessonOrder = lessonDataDto.LessonOrder,

            ContentType = lessonDataDto.ContentType.ToString(),

            DurationInMinutes = lessonDataDto.DurationInMinutes ?? 0,
            VideoUrl = lessonDataDto.VideoUrl ?? "",
            ArticleContent = lessonDataDto.ArticleContent ?? "",

            Resources = lessonDataDto.Resources.Select(r => new ManageEditLessonResourceViewModel
            {
                LessonResourceId = r.LessonResourceId,
                ResourceType = r.ResourceType.ToString() ?? "",
                Title = r.Title ?? "---",
                Url = r.Url
            })
        };

        return Json(viewModel);
    }

    // ============================================
    // MANAGE COURSE - MODULE ENDPOINTS
    // ============================================

    [HttpPost("/instructor/manage-course/{courseId:int}/module")]
    public async Task<IActionResult> SaveModuleAsync(int courseId, [FromBody] ManageSaveModuleDto moduleDto)
    {
        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Json(new { success = false, message = "Unauthorized" });

        moduleDto.CourseId = courseId;
        var result = await _instructorManageCourseService.SaveModuleAsync(instructorId, courseId, moduleDto);

        if (result.Success)
            return Json(new { success = true, moduleId = result.EntityId, message = "Module saved successfully" });

        return Json(new { success = false, message = result.Message ?? "Failed to save module" });
    }

    [HttpDelete("/instructor/manage-course/{courseId:int}/module/{moduleId:int}")]
    public async Task<IActionResult> DeleteModuleAsync(int courseId, int moduleId)
    {
        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Json(new { success = false, message = "Unauthorized" });

        var result = await _instructorManageCourseService.DeleteModuleAsync(instructorId, courseId, moduleId);

        if (result)
            return Json(new { success = true, message = "Module deleted successfully" });

        return Json(new { success = false, message = "Failed to delete module" });
    }

    // ============================================
    // MANAGE COURSE - LESSON ENDPOINTS
    // ============================================

    [HttpPost("/instructor/manage-course/{courseId:int}/lesson")]
    public async Task<IActionResult> SaveLessonAsync(int courseId, [FromBody] ManageSaveLessonDto lessonDto)
    {
        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Json(new { success = false, message = "Unauthorized" });

        lessonDto.CourseId = courseId;
        var result = await _instructorManageCourseService.SaveLessonAsync(instructorId, courseId, lessonDto);

        if (result.Success)
            return Json(new { success = true, lessonId = result.EntityId, message = "Lesson saved successfully" });

        return Json(new { success = false, message = result.Message ?? "Failed to save lesson" });
    }

    [HttpDelete("/instructor/manage-course/{courseId:int}/lesson/{lessonId:int}")]
    public async Task<IActionResult> DeleteLessonAsync(int courseId, int lessonId)
    {
        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Json(new { success = false, message = "Unauthorized" });

        var result = await _instructorManageCourseService.DeleteLessonAsync(instructorId, courseId, lessonId);

        if (result)
            return Json(new { success = true, message = "Lesson deleted successfully" });

        return Json(new { success = false, message = "Failed to delete lesson" });
    }

    [HttpGet("/instructor/manage-course/{courseId:int}/module/{moduleId:int}/lesson/{lessonId:int}/modal")]
    public async Task<IActionResult> GetLessonModalAsync(int courseId, int moduleId, int lessonId)
    {
        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Unauthorized();

        // Verify course ownership
        var hasCourse = await _instructorProfileService.HasCourseWithIdAsync(instructorId, courseId);
        if (!hasCourse)
            return Forbid();

        ManageEditLessonViewModel viewModel;

        if (lessonId == 0)
        {
            // New lesson - return empty form
            viewModel = new ManageEditLessonViewModel
            {
                CourseId = courseId,
                ModuleId = moduleId,
                LessonId = 0,
                ContentType = "Video",
                Resources = []
            };
        }
        else
        {
            // Existing lesson - fetch data
            var lessonData = await _instructorManageCourseService.GetLessonDataAsync(instructorId, lessonId);
            if (lessonData == null)
                return NotFound();

            viewModel = new ManageEditLessonViewModel
            {
                CourseId = courseId,
                ModuleId = moduleId,
                LessonId = lessonData.LessonId,
                LessonTitle = lessonData.LessonTitle,
                LessonOrder = lessonData.LessonOrder,
                ContentType = lessonData.ContentType.ToString(),
                VideoUrl = lessonData.VideoUrl,
                DurationInMinutes = lessonData.DurationInMinutes,
                ArticleContent = lessonData.ArticleContent,
                Resources = lessonData.Resources.Select(r => new ManageEditLessonResourceViewModel
                {
                    LessonResourceId = r.LessonResourceId,
                    ResourceType = r.ResourceType.ToString(),
                    Title = r.Title,
                    Url = r.Url
                })
            };
        }

        return PartialView("_ManageCourseLessonModalPartialView", viewModel);
    }

    private string FormatDuration(int durationInMinutes)
    {
        int h = durationInMinutes / 60;
        int m = durationInMinutes % 60;

        List<string> parts = new();

        if (h > 0)
            parts.Add($"{h} {(h == 1 ? "hr" : "hrs")}");

        if (m > 0)
            parts.Add($"{m} {(m == 1 ? "min" : "mins")}");

        return parts.Count > 0 ? string.Join(" ", parts) : "---";
    }

    /// <summary>
    /// Submit Course For Review - Submit the course for admin review.
    /// </summary>
    [HttpPost("/instructor/manage-course/{courseId:int}/submit-for-review")]
    public async Task<IActionResult> SubmitCourseForReviewAsync(int courseId)
    {
        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Json(new { success = false, message = "Unauthorized" });

        // Verify course ownership
        var hasCourse = await _instructorProfileService.HasCourseWithIdAsync(instructorId, courseId);
        if (!hasCourse)
            return Json(new { success = false, message = "Course not found" });

        // Get the course and update status
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null)
            return Json(new { success = false, message = "Course not found" });

        // Only allow submission if course is in Draft status
        if (course.Status != LearningEntityStatus.Draft)
            return Json(new { success = false, message = "Only draft courses can be submitted for review" });

        // Update status to Pending
        course.Status = LearningEntityStatus.Pending;
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Course submitted for review successfully!" });
    }

    /// <summary>
    /// Save Basic Info - Update course title, description, categories, level, and languages.
    /// </summary>
    [HttpPost("/instructor/manage-course/{courseId:int}/basic-info")]
    public async Task<IActionResult> SaveBasicInfoAsync(int courseId, [FromBody] ManageSaveBasicInfoDto basicInfoDto)
    {
        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Json(new { success = false, message = "Unauthorized" });

        // Verify course ownership
        var hasCourse = await _instructorProfileService.HasCourseWithIdAsync(instructorId, courseId);
        if (!hasCourse)
            return Json(new { success = false, message = "Course not found" });

        basicInfoDto.CourseId = courseId;
        var result = await _instructorManageCourseService.SaveBasicInfoAsync(instructorId, courseId, basicInfoDto);

        if (result.Success)
            return Json(new { success = true, message = result.Message ?? "Basic info saved successfully" });

        return Json(new { success = false, message = result.Message ?? "Failed to save basic info" });
    }

    // ============================================
    // MANAGE COURSE - LEARNING OUTCOMES ENDPOINTS
    // ============================================

    /// <summary>
    /// Save Learning Outcome - Add or update a learning outcome for the course.
    /// </summary>
    [HttpPost("/instructor/manage-course/{courseId:int}/learning-outcome")]
    public async Task<IActionResult> SaveLearningOutcomeAsync(int courseId, [FromBody] ManageSaveLearningOutcomeDto outcomeDto)
    {
        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Json(new { success = false, message = "Unauthorized" });

        // Verify course ownership
        var hasCourse = await _instructorProfileService.HasCourseWithIdAsync(instructorId, courseId);
        if (!hasCourse)
            return Json(new { success = false, message = "Course not found" });

        outcomeDto.CourseId = courseId;
        var result = await _instructorManageCourseService.SaveLearningOutcomeAsync(instructorId, courseId, outcomeDto);

        if (result.Success)
            return Json(new { success = true, outcomeId = result.EntityId, message = result.Message ?? "Learning outcome saved successfully" });

        return Json(new { success = false, message = result.Message ?? "Failed to save learning outcome" });
    }

    /// <summary>
    /// Delete Learning Outcome - Remove a learning outcome from the course.
    /// </summary>
    [HttpDelete("/instructor/manage-course/{courseId:int}/learning-outcome/{outcomeId:int}")]
    public async Task<IActionResult> DeleteLearningOutcomeAsync(int courseId, int outcomeId)
    {
        var instructorId = _currentUserService.GetInstructorId();
        if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
            return Json(new { success = false, message = "Unauthorized" });

        // Verify course ownership
        var hasCourse = await _instructorProfileService.HasCourseWithIdAsync(instructorId, courseId);
        if (!hasCourse)
            return Json(new { success = false, message = "Course not found" });

        var result = await _instructorManageCourseService.DeleteLearningOutcomeAsync(instructorId, courseId, outcomeId);

        if (result)
            return Json(new { success = true, message = "Learning outcome deleted successfully" });

        return Json(new { success = false, message = "Failed to delete learning outcome" });
    }
}