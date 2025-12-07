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
using Core.Entities.Enums;
using BLL.DTOs.Instructor;
using Microsoft.AspNetCore.Identity;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Core.RepositoryInterfaces;
using Web.Interfaces;
using DAL.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    /// Edit Course - Modify course details, content, and settings.
    /// </summary>
    [HttpGet("/instructor/edit-course/{courseId}")]
    public async Task<IActionResult> EditCourse(int courseId)
    {
        try
        {
            var instructorId = await GetInstructorIdAsync();
            if (instructorId == 0)
                return Unauthorized("Instructor profile not found");

            Console.WriteLine($"Loading course {courseId} for instructor {instructorId}");

            var courseData = await _coursesService.GetCourseForEditAsync(instructorId, courseId);
            
            if (courseData == null)
            {
                Console.WriteLine($"Course not found: {courseId}");
                return NotFound();
            }

            // Load categories from database
            var categories = await _categoryRepository.GetAllAsync();

            Console.WriteLine($"Course loaded: {courseData.Title}");
            Console.WriteLine($"Category: {courseData.MainCategory?.Name ?? "None"}");

            var viewModel = await BuildEditCourseViewModelAsync(courseData, categories);
            
            Console.WriteLine($"ViewModel built successfully. Selected Category ID: {viewModel.Data.SelectedCategoryId}");
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in EditCourse GET: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, "An error occurred while loading the course");
        }
    }

    /// <summary>
    /// Edit Course - Handle course updates from the form.
    /// </summary>
    [HttpPost("/instructor/edit-course/{courseId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(int courseId, [FromForm] EditCourseFormModel formModel)
    {
        Console.WriteLine($"📝 Form Data Received:");
        Console.WriteLine($"CourseTitle: {formModel.CourseTitle}");
        Console.WriteLine($"Description: {formModel.Description}");
        Console.WriteLine($"CategoryId: {formModel.CategoryId}");
        Console.WriteLine($"Level: {formModel.Level}");
        Console.WriteLine($"ThumbnailUrl: {formModel.ThumbnailUrl}");
        Console.WriteLine($"LearningOutcomes Count: {formModel.LearningOutcomes?.Count ?? 0}");
        
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized("Instructor profile not found");
            
        if (!ModelState.IsValid)
        {
            var courseData = await _coursesService.GetCourseForEditAsync(instructorId, courseId);
            if (courseData == null)
                return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            var viewModel = await BuildEditCourseViewModelAsync(courseData, categories, formModel);
            return View(viewModel);
        }

        var result = await _coursesService.UpdateCourseAsync(instructorId, courseId, new UpdateCourseDto
        {
            Title = formModel.CourseTitle,
            Description = formModel.Description,
            Level = Enum.Parse<CourseLevel>(formModel.Level, true),
            CategoryId = int.Parse(formModel.CategoryId),
            ThumbnailUrl = formModel.ThumbnailUrl,
            LearningOutcomes = formModel.LearningOutcomes ?? new List<string>()
        });

        if (result)
        {
            TempData["SuccessMessage"] = "Course updated successfully!";
            return RedirectToAction("EditCourse", new { courseId });
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update course.";
            var courseData = await _coursesService.GetCourseForEditAsync(instructorId, courseId);
            if (courseData == null)
                return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            var viewModel = await BuildEditCourseViewModelAsync(courseData, categories, formModel);
            return View(viewModel);
        }
    }

    // Helper method to build the EditCourseViewModel
    private Task<EditCourseViewModel> BuildEditCourseViewModelAsync(
        InstructorCourseEditDto courseData,
        IEnumerable<Category> categories,
        EditCourseFormModel? formModel = null)
    {
        // Helper method to format duration
        string FormatDuration(int seconds)
        {
            var hours = seconds / 3600;
            var minutes = (seconds % 3600) / 60;
            if (hours > 0)
                return $"{hours}h {minutes}m";
            return $"{minutes}m";
        }

        // Helper method to get lesson type info
        (string label, string cssClass, string icon) GetLessonTypeInfo(LessonContentType type)
        {
            return type switch
            {
                LessonContentType.Video => ("Video", "video", "fa-play-circle"),
                LessonContentType.Article => ("Article", "article", "fa-file-alt"),
                _ => ("Unknown", "unknown", "fa-file")
            };
        }

        // Helper method to calculate relative time
        string GetRelativeTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue) return "Never";
            
            var timeSpan = DateTime.Now - dateTime.Value;
            
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} days ago";
            
            return dateTime.Value.ToString("MMM dd, yyyy");
        }

        string selectedCategoryId = "";
        if (formModel != null && !string.IsNullOrEmpty(formModel.CategoryId))
        {
            selectedCategoryId = formModel.CategoryId;
            Console.WriteLine($"Using category from form model: {selectedCategoryId}");
        }
        else if (courseData.MainCategory != null)
        {
            selectedCategoryId = courseData.MainCategory.CategoryId.ToString();
            Console.WriteLine($"Using category from course data: {selectedCategoryId} ({courseData.MainCategory.Name})");
        }

        var viewModel = new EditCourseViewModel
        {
            Data = new EditCourseDataViewModel
            {
                CourseId = courseData.CourseId,
                CourseTitle = formModel?.CourseTitle ?? courseData.Title,
                Description = formModel?.Description ?? courseData.Description,
                ThumbnailUrl = formModel?.ThumbnailUrl ?? courseData.ThumbnailImageUrl,
                
                SelectedCategoryId = selectedCategoryId,
                SelectedLevel = formModel?.Level ?? ((int)courseData.Level).ToString(),
                
                AvailableCategories = categories
                    .Select(c => new SelectOption 
                    { 
                        Value = c.CategoryId.ToString(), 
                        Text = c.Name 
                    })
                    .ToList(),
                
                AvailableLevels = new List<SelectOption>
                {
                    new SelectOption { Value = "2", Text = "Beginner" },
                    new SelectOption { Value = "3", Text = "Intermediate" },
                    new SelectOption { Value = "4", Text = "Advanced" }
                },
                
                LearningOutcomes = courseData.LearningOutcomes?
                    .Select(lo => lo.Title ?? lo.Description ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList() ?? new List<string>(),
                
                Stats = new EditCourseStatsViewModel
                {
                    EnrolledStudents = courseData.EnrolledStudents,
                    AverageRating = courseData.AverageRating,
                    Completions = courseData.Completions,
                    AverageProgress = courseData.AverageProgress
                },
                
                Modules = courseData.Modules?
                    .Select(m => new EditModuleViewModel
                    {
                        ModuleId = m.ModuleId,
                        Title = m.Title ?? string.Empty,
                        Description = m.Description ?? string.Empty,
                        Order = m.Order,
                        LessonsCount = m.LessonsCount,
                        DurationFormatted = FormatDuration(m.TotalDurationSeconds),
                        Lessons = m.Lessons?
                            .Select(l =>
                            {
                                var lessonTypeInfo = GetLessonTypeInfo(l.ContentType);
                                return new EditLessonViewModel
                                {
                                    LessonId = l.LessonId,
                                    Title = l.Title ?? string.Empty,
                                    Order = l.Order,
                                    ContentType = (int)l.ContentType,
                                    VideoUrl = l.VideoUrl,
                                    PdfUrl = l.ArticleContent,
                                    DurationInSeconds = l.DurationInSeconds,
                                    TypeLabel = lessonTypeInfo.label,
                                    TypeClass = lessonTypeInfo.cssClass,
                                    TypeIcon = lessonTypeInfo.icon,
                                    DurationFormatted = l.ContentType == LessonContentType.Video 
                                        ? FormatDuration(l.DurationInSeconds)
                                        : "N/A",
                                    Resources = l.Resources?
                                        .Select(r => new EditLessonResourceViewModel
                                        {
                                            LessonResourceId = r.LessonResourceId,
                                            ResourceType = (int)r.ResourceType,
                                            ResourceTypeName = r.ResourceType.ToString(),
                                            Url = r.Url ?? string.Empty,
                                            Title = r.Title
                                        })
                                        .ToList() ?? new List<EditLessonResourceViewModel>()
                                };
                            })
                            .ToList() ?? new List<EditLessonViewModel>()
                    })
                    .ToList() ?? new List<EditModuleViewModel>(),
                
                EnrolledStudents = courseData.Students?
                    .Select(s => new EnrolledStudentViewModel
                    {
                        StudentId = s.StudentId,
                        Name = $"{s.FirstName ?? "Unknown"} {s.LastName ?? "User"}".Trim(),
                        Email = s.Email ?? string.Empty,
                        Initials = $"{s.FirstName?.FirstOrDefault() ?? 'U'}{s.LastName?.FirstOrDefault() ?? 'U'}".ToUpper(),
                        EnrolledDate = s.EnrollmentDate?.ToString("MMM dd, yyyy") ?? "N/A",
                        ProgressPercentage = (int)s.ProgressPercentage,
                        LastActivity = GetRelativeTime(s.LastAccessDate),
                        Status = s.ProgressPercentage >= 100 ? "Completed" : "Active",
                        StatusClass = s.ProgressPercentage >= 100 ? "completed" : "active"
                    })
                    .ToList() ?? new List<EnrolledStudentViewModel>(),
                
                TotalStudentPages = (int)Math.Ceiling((courseData.EnrolledStudents) / 25.0)
            },
            
            PageTitle = "Edit Course"
        };

        return Task.FromResult(viewModel);
    }

    [HttpPost("/instructor/edit-course/{courseId}/module")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateModule(int courseId, [FromBody] UpdateModuleDto moduleDto)
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Json(new { success = false, message = "Instructor profile not found" });

        var result = await _coursesService.UpdateModuleAsync(instructorId, courseId, moduleDto);

        if (result.Success)
            return Json(new { success = true, message = "Module saved successfully", moduleId = result.ModuleId });
        else
            return Json(new { success = false, message = result.Message ?? "Failed to save module" });
    }

    [HttpDelete("/instructor/edit-course/{courseId}/module/{moduleId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModule(int courseId, int moduleId)
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Json(new { success = false, message = "Instructor profile not found" });

        var result = await _coursesService.DeleteModuleAsync(instructorId, courseId, moduleId);

        if (result)
            return Json(new { success = true, message = "Module deleted successfully" });
        else
            return Json(new { success = false, message = "Failed to delete module" });
    }

    [HttpPost("/instructor/edit-course/{courseId}/lesson")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLesson(int courseId, [FromBody] UpdateLessonDto lessonDto)
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Json(new { success = false, message = "Instructor profile not found" });

        var result = await _coursesService.UpdateLessonAsync(instructorId, courseId, lessonDto);

        if (result.Success)
            return Json(new { success = true, message = "Lesson saved successfully", lessonId = result.LessonId });
        else
            return Json(new { success = false, message = result.Message ?? "Failed to save lesson" });
    }

    [HttpDelete("/instructor/edit-course/{courseId}/lesson/{lessonId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLesson(int courseId, int lessonId)
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Json(new { success = false, message = "Instructor profile not found" });

        var result = await _coursesService.DeleteLessonAsync(instructorId, courseId, lessonId);

        if (result)
            return Json(new { success = true, message = "Lesson deleted successfully" });
        else
            return Json(new { success = false, message = "Failed to delete lesson" });
    }

    [HttpPost("/instructor/upload-course-thumbnail")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadCourseThumbnail(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, message = "No file uploaded" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return Json(new { success = false, message = "Invalid file type" });

        if (file.Length > 5 * 1024 * 1024)
            return Json(new { success = false, message = "File size exceeds 5MB" });

        try
        {
            var fileName = $"course-thumbnail-{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "thumbnails");
            
            Directory.CreateDirectory(uploadsFolder);
            
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            var fileUrl = $"/uploads/thumbnails/{fileName}";
            return Json(new { success = true, url = fileUrl });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Upload failed: " + ex.Message });
        }
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


        var viewModel = new ManageCourseViewModel
        {
            CourseId = data.CourseId,
            CourseTitle = data.CourseTitle,
            CourseStatus = data.CourseStatus.ToString(),

            Curriculum = new ManageCourseCurriculumViewModel
            {
                CourseId = data.CourseId,
                CourseTitle = data.CourseTitle,
                CourseStatus = data.CourseStatus.ToString(),

                Modules = data.Curriculum.Modules.Select(m => new ManageViewModuleViewModel
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
                    TotalModules = data.Curriculum.Modules.Count(),

                    VideoLessonsCount = data.Curriculum.Modules
                        .SelectMany(m => m.Lessons)
                        .Count(l => l.ContentType == LessonContentType.Video),

                    ArticleLessonsCount = data.Curriculum.Modules
                        .SelectMany(m => m.Lessons)
                        .Count(l => l.ContentType == LessonContentType.Article),

                    FormattedCourseDuration = FormatDuration(data.Curriculum.Modules
                        .Where(m => m.DurationInMinutes != null)
                        .Sum(m => m.DurationInMinutes) ?? 0
                    )
                }
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

    //[HttpPost("/instructor/manage-course/lesson/{lessonId:int:required}")]
    //public async Task<IActionResult> UpdateLessonDataAsync(int lessonId, [FromBody] ManageEditLessonDto newLessonData)
    //{
    //    if (!ModelState.IsValid)
    //        return NotFound("requested lesson is not found");

    //    // authenticate the user
    //    var instructorId = _currentUserService.GetInstructorId();
    //    if (instructorId == 0 || !await _userRepository.HasInstructorProfileWithIdAsync(instructorId))
    //        return Unauthorized("user is not an instructor");

    //    //authorization to access the course and its lessons
    //    var courseId = await _instructorProfileService.GetCourseIdForLessonAsync(instructorId, lessonId);
    //    if (courseId == null)
    //        return Forbid();

    //    // authenticated and authorized ==> update lesson data
    //    _coursesService.UpdateLessonAsync()
    //}



    //[HttpPost("/instructor/manage-course/{courseId:int:required}/curriculum")]
    //public IActionResult SaveCourseCurriculumPartialAsync(ManageCourseCurriculumViewModel model)
    //{

    //}

    //[HttpPost("/instructor/manage-course/{courseId:int:required}")]
    //[ValidateAntiForgeryToken]
    //public IActionResult ManageCourse(int courseId)
    //{

    //    return View();
    //}




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
}




// Course Name
//  -)
//      Header (Course Title, Status)       =============> Never changes unless page refreshed

//  1) Curriculum 
//      CourseCurriculumDto ----> List<ModuleDto> ---> ModuleDto has List<LessonDto> ---> having (name, type, duration)
//      CourseStatsDto ---->
//          1) Total Modules (first time from the data base and then from the js (or just everytime from the database))
//          2) Video Lessons 
//          3) Article Lessons
//          4) Total Course Duration
//          5) Total Number of Students enrolled   XXXXXXXXXXXXXXXXXXXXXXXXXXX


//  2) Basic Info
//      Course Title
//      Course Description
//      Categories
//      Level
//      Languages
//      
//      Course Summary
//          - Languages
//          - Level
//          - Main Category

// 3) Learning Outcomes
//      Course Learning Outcomes;