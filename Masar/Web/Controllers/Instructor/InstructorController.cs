using BLL.DTOs.Misc;
using BLL.Helpers;
using BLL.Interfaces.Instructor;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Instructor;
using Web.ViewModels.Instructor.Dashboard;
using Web.ViewModels.Misc;
using Core.Entities.Enums;
using BLL.DTOs.Instructor;
using BLL.DTOs.Course;
using Microsoft.AspNetCore.Identity;
using Core.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Core.RepositoryInterfaces;
using Web.Interfaces;

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
    private readonly ICategoryRepository _categoryRepository;  // ADD THIS

    public InstructorController(
        IInstructorDashboardService dashboardService, 
        IInstructorCoursesService courseService,
        IInstructorProfileService profileService,
        RazorViewToStringRenderer razorRenderer,
        UserManager<User> userManager,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ICategoryRepository categoryRepository)  // ADD THIS PARAMETER
    {
        _dashboardService = dashboardService;
        _coursesService = courseService;
        _profileService = profileService;
        _razorRenderer = razorRenderer;
        _userManager = userManager;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _categoryRepository = categoryRepository;  // ADD THIS
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
                    Completions = dashboardData.CurrentMonthStats.Completions
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
                    new CourseCardViewModel
                    {
                        CourseId = c.CourseId,
                        Title = c.Title,
                        Description = c.Description,
                        
                        MainCategory = c.MainCategory?.Name ??  "Undefined",
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
            
        var initialRequest = new PagingRequestDto() { CurrentPage = 1, PageSize = 3 };
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

                    CreatedDate = c.CreatedDate.ToString("dd-MM-yyyy"),
                    LastUpdatedDate = c.LastUpdatedDate.ToString("dd-MM-yyyy"),

                    Level = c.Level.ToString(),
                    MainCategory = c.MainCategory?.Name ?? "Undefined",

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

                CreatedDate = c.CreatedDate.ToString("MM-dd-yyyy"),
                LastUpdatedDate = c.LastUpdatedDate.ToString("MM-dd-yyyy"),

                Level = c.Level.ToString(),
                MainCategory = c.MainCategory?.Name ?? "Undefined",

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
                Location = profileData.Location,
                Languages = profileData.Languages,
                JoinedDate = profileData.JoinedDate.ToString("MMMM yyyy"),

                GithubUrl = profileData.GithubUrl,
                LinkedInUrl = profileData.LinkedInUrl,
                TwitterUrl = profileData.TwitterUrl,
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

                TeachingExpertise = profileData.TeachingExpertise,

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
        // Add debugging
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
            // Reload the course data
            var courseData = await _coursesService.GetCourseForEditAsync(instructorId, courseId);
            if (courseData == null)
                return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            var viewModel = await BuildEditCourseViewModelAsync(courseData, categories, formModel);
            return View(viewModel);
        }

        // Save the changes
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

    // Updated helper method to load categories from database
    private Task<EditCourseViewModel> BuildEditCourseViewModelAsync(
        BLL.DTOs.Instructor.InstructorCourseEditDto courseData,
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
                
                // Load categories from database instead of hardcoded values
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
                                    PdfUrl = l.PdfUrl,
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
            return Unauthorized("Instructor profile not found");

        var result = await _coursesService.UpdateModuleAsync(instructorId, courseId, moduleDto);

        if (result)
            return Json(new { success = true, message = "Module updated successfully" });
        else
            return Json(new { success = false, message = "Failed to update module" });
    }

    [HttpDelete("/instructor/edit-course/{courseId}/module/{moduleId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModule(int courseId, int moduleId)
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized("Instructor profile not found");

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
            return Unauthorized("Instructor profile not found");

        var result = await _coursesService.UpdateLessonAsync(instructorId, courseId, lessonDto);

        if (result)
            return Json(new { success = true, message = "Lesson updated successfully" });
        else
            return Json(new { success = false, message = "Failed to update lesson" });
    }

    [HttpDelete("/instructor/edit-course/{courseId}/lesson/{lessonId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLesson(int courseId, int lessonId)
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized("Instructor profile not found");

        var result = await _coursesService.DeleteLessonAsync(instructorId, courseId, lessonId);

        if (result)
            return Json(new { success = true, message = "Lesson deleted successfully" });
        else
            return Json(new { success = false, message = "Failed to delete lesson" });
    }

    /// <summary>
    /// UploadCourseThumbnail - AJAX endpoint to upload course thumbnail images.
    /// </summary>
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
}
