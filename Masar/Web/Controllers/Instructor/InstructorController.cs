using BLL.DTOs.Misc;
using BLL.Helpers;
using BLL.Interfaces.Instructor;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Instructor;
using Web.ViewModels.Instructor.Dashboard;
using Web.ViewModels.Misc;
using Core.Entities.Enums;
using BLL.DTOs.Instructor;
using Microsoft.AspNetCore.Identity;
using Core.Entities;
using System.Security.Claims;

namespace Web.Controllers.Instructor;

public class InstructorController : Controller
{
    private readonly IInstructorDashboardService _dashboardService;
    private readonly IInstructorCoursesService _coursesService;
    private readonly IInstructorProfileService _profileService;
    private readonly RazorViewToStringRenderer _razorRenderer;
    private readonly int instructorId = 1;     // TODO: Get from logged in user context
    private readonly UserManager<User> _userManager;
    private readonly int instructorId = 1;     // TODO: Get from logged in user context

    public InstructorController(
        IInstructorDashboardService dashboardService, 
        IInstructorCoursesService courseService,
        IInstructorProfileService profileService,
        RazorViewToStringRenderer razorRenderer,
        UserManager<User> userManager)
    {
        _dashboardService = dashboardService;
        _coursesService = courseService;
        _profileService = profileService;
        _razorRenderer = razorRenderer;
        _userManager = userManager;
    }

    // Helper method to get instructor ID from logged-in user
    private async Task<int> GetInstructorIdAsync()
    {
        // TODO: Replace this temporary hardcoded value with actual authentication
        // For now, return instructor ID 1 which matches the seeded data
        // After implementing authentication, use this code:
        /*
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return 0;
        
        var user = await _userManager.Users
            .Include(u => u.InstructorProfile)
            .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
            
        return user?.InstructorProfile?.InstructorId ?? 0;
        */
        
        return 1; // Matches john.instructor@example.com from seed data
    }


    [HttpGet("/instructor/dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Title = "Instructor Dashboard | Masar";

        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized();
            
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


    [HttpGet("/instructor/my-courses")]
    public async Task<IActionResult> MyCourses()
    {
        ViewBag.Title = "My Courses | Masar";
        
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized();
            
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
                    MainCategory = c.MainCategory.Name ?? "Undefined",

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


    [HttpPost("/instructor/my-courses")]
    public async Task<IActionResult> GetCoursesPartial([FromBody] PagingRequestDto request)
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized();
            
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
                MainCategory = c.MainCategory.Name ?? "Undefined",

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

    [HttpGet("/instructor/profile")]
    public async Task<IActionResult> Profile()
    {
        ViewBag.Title = "Instructor Profile | Masar";

        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized();
            
        var profileData = await _profileService.GetInstructorProfileAsync(instructorId);

        if (profileData == null)
        {
            return NotFound();
        }

        // Generate random gradients for courses
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

    [HttpGet("/instructor/edit-course/{courseId}")]
    public async Task<IActionResult> EditCourse(int courseId)
    {
        ViewBag.Title = "Edit Course | Masar";

        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized();

        // Fetch actual course data from database
        var courseData = await _coursesService.GetCourseForEditAsync(instructorId, courseId);

        if (courseData == null)
        {
            return NotFound("Course not found or you don't have permission to edit it.");
        }

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

        var viewModel = new EditCourseViewModel
        {
            Data = new EditCourseDataViewModel
            {
                CourseId = courseData.CourseId,
                CourseTitle = courseData.Title,
                Description = courseData.Description,
                ThumbnailUrl = courseData.ThumbnailImageUrl,
                
                SelectedCategoryId = courseData.MainCategory?.CategoryId.ToString() ?? "5",
                SelectedLevel = courseData.Level.ToString().ToLower(),
                
                AvailableCategories = new List<SelectOption>
                {
                    new SelectOption { Value = "5", Text = "Web Development" },
                    new SelectOption { Value = "6", Text = "Mobile Development" },
                    new SelectOption { Value = "2", Text = "Data Science" },
                    new SelectOption { Value = "8", Text = "Programming Languages" },
                    new SelectOption { Value = "3", Text = "Design" },
                    new SelectOption { Value = "9", Text = "Frontend" },
                    new SelectOption { Value = "10", Text = "Backend" },
                    new SelectOption { Value = "12", Text = "Machine Learning" }
                },
                
                AvailableLevels = new List<SelectOption>
                {
                    new SelectOption { Value = "beginner", Text = "Beginner" },
                    new SelectOption { Value = "intermediate", Text = "Intermediate" },
                    new SelectOption { Value = "advanced", Text = "Advanced" }
                },
                
                LearningOutcomes = courseData.LearningOutcomes
                    .Select(lo => lo.Title)
                    .ToList(),
                
                Stats = new EditCourseStatsViewModel
                {
                    EnrolledStudents = courseData.EnrolledStudents,
                    AverageRating = courseData.AverageRating,
                    Completions = courseData.Completions,
                    AverageProgress = courseData.AverageProgress
                },
                
                Modules = courseData.Modules.Select(m => new EditModuleViewModel
                {
                    ModuleId = m.ModuleId,
                    Title = m.Title,
                    Description = m.Description,
                    Order = m.Order,
                    LessonsCount = m.LessonsCount,
                    DurationFormatted = FormatDuration(m.TotalDurationSeconds),
                    Lessons = m.Lessons.Select(l =>
                    {
                        var lessonTypeInfo = GetLessonTypeInfo(l.ContentType);
                        return new EditLessonViewModel
                        {
                            LessonId = l.LessonId,
                            Title = l.Title,
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
                            Resources = l.Resources.Select(r => new EditLessonResourceViewModel
                            {
                                LessonResourceId = r.LessonResourceId,
                                ResourceType = (int)r.ResourceType,
                                ResourceTypeName = r.ResourceType.ToString(),
                                Url = r.Url,
                                Title = r.Title
                            }).ToList()
                        };
                    }).ToList()
                }).ToList(), // <-- THIS IS THE FIX: Added closing brace and comma here
            
                EnrolledStudents = courseData.Students.Select(s => new EnrolledStudentViewModel
                {
                    StudentId = s.StudentId,
                    Name = $"{s.FirstName} {s.LastName}",
                    Email = s.Email,
                    Initials = $"{s.FirstName.FirstOrDefault()}{s.LastName.FirstOrDefault()}".ToUpper(),
                    EnrolledDate = s.EnrollmentDate?.ToString("MMM dd, yyyy") ?? "N/A",
                    ProgressPercentage = (int)s.ProgressPercentage,
                    LastActivity = GetRelativeTime(s.LastAccessDate),
                    Status = s.ProgressPercentage >= 100 ? "Completed" : "Active",
                    StatusClass = s.ProgressPercentage >= 100 ? "completed" : "active"
                }).ToList(),
                
                TotalStudentPages = (int)Math.Ceiling(courseData.EnrolledStudents / 25.0)
            },
            
            PageTitle = "Edit Course"
        };

        return View(viewModel);
    }

    [HttpPost("/instructor/edit-course/{courseId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(int courseId, [FromForm] EditCourseFormModel formModel)
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized();
            
        if (!ModelState.IsValid)
        {
            // Reload the course data
            var courseData = await _coursesService.GetCourseForEditAsync(instructorId, courseId);
            if (courseData == null)
                return NotFound();

            // Rebuild the view model with form data and validation errors
            var viewModel = BuildEditCourseViewModel(courseData, formModel);
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
                
            var viewModel = BuildEditCourseViewModel(courseData, formModel);
            return View(viewModel);
        }
    }

    // Helper method to build view model
    private EditCourseViewModel BuildEditCourseViewModel(
        BLL.DTOs.Instructor.InstructorCourseEditDto courseData, 
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

        var viewModel = new EditCourseViewModel
        {
            Data = new EditCourseDataViewModel
            {
                CourseId = courseData.CourseId,
                CourseTitle = formModel?.CourseTitle ?? courseData.Title,
                Description = formModel?.Description ?? courseData.Description,
                ThumbnailUrl = formModel?.ThumbnailUrl ?? courseData.ThumbnailImageUrl,
                
                SelectedCategoryId = formModel?.CategoryId ?? courseData.MainCategory?.CategoryId.ToString() ?? "5",
                SelectedLevel = formModel?.Level ?? courseData.Level.ToString().ToLower(),
                
                // FIXED: Match seeded category IDs from SQL script
                AvailableCategories = new List<SelectOption>
                {
                    new SelectOption { Value = "5", Text = "Web Development" },
                    new SelectOption { Value = "6", Text = "Mobile Development" },
                    new SelectOption { Value = "2", Text = "Data Science" },
                    new SelectOption { Value = "8", Text = "Programming Languages" },
                    new SelectOption { Value = "3", Text = "Design" },
                    new SelectOption { Value = "9", Text = "Frontend" },
                    new SelectOption { Value = "10", Text = "Backend" },
                    new SelectOption { Value = "12", Text = "Machine Learning" }
                },
                
                AvailableLevels = new List<SelectOption>
                {
                    new SelectOption { Value = "beginner", Text = "Beginner" },
                    new SelectOption { Value = "intermediate", Text = "Intermediate" },
                    new SelectOption { Value = "advanced", Text = "Advanced" }
                },
                
                LearningOutcomes = formModel?.LearningOutcomes ?? 
                    courseData.LearningOutcomes.Select(lo => lo.Title).ToList(),
                
                Stats = new EditCourseStatsViewModel
                {
                    EnrolledStudents = courseData.EnrolledStudents,
                    AverageRating = courseData.AverageRating,
                    Completions = courseData.Completions,
                    AverageProgress = courseData.AverageProgress
                },
                
                Modules = courseData.Modules.Select(m => new EditModuleViewModel
                {
                    ModuleId = m.ModuleId,
                    Title = m.Title,
                    Description = m.Description,
                    Order = m.Order,
                    LessonsCount = m.LessonsCount,
                    DurationFormatted = FormatDuration(m.TotalDurationSeconds),
                    Lessons = m.Lessons.Select(l =>
                    {
                        var lessonTypeInfo = GetLessonTypeInfo(l.ContentType);
                        return new EditLessonViewModel
                        {
                            LessonId = l.LessonId,
                            Title = l.Title,
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
                            Resources = l.Resources.Select(r => new EditLessonResourceViewModel
                            {
                                LessonResourceId = r.LessonResourceId,
                                ResourceType = (int)r.ResourceType,
                                ResourceTypeName = r.ResourceType.ToString(),
                                Url = r.Url,
                                Title = r.Title
                            }).ToList()
                        };
                    }).ToList()
                }).ToList(),
                
                EnrolledStudents = courseData.Students.Select(s => new EnrolledStudentViewModel
                {
                    StudentId = s.StudentId,
                    Name = $"{s.FirstName} {s.LastName}",
                    Email = s.Email,
                    Initials = $"{s.FirstName.FirstOrDefault()}{s.LastName.FirstOrDefault()}".ToUpper(),
                    EnrolledDate = s.EnrollmentDate?.ToString("MMM dd, yyyy") ?? "N/A",
                    ProgressPercentage = (int)s.ProgressPercentage,
                    LastActivity = GetRelativeTime(s.LastAccessDate),
                    Status = s.ProgressPercentage >= 100 ? "Completed" : "Active",
                    StatusClass = s.ProgressPercentage >= 100 ? "completed" : "active"
                }).ToList(),
                
                TotalStudentPages = (int)Math.Ceiling(courseData.EnrolledStudents / 25.0)
            },
            
            PageTitle = "Edit Course"
        };

        return viewModel;
    }

    [HttpPost("/instructor/edit-course/{courseId}/module")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateModule(int courseId, [FromBody] UpdateModuleDto moduleDto)
    {
        var instructorId = await GetInstructorIdAsync();
        if (instructorId == 0)
            return Unauthorized();

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
            return Unauthorized();

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
            return Unauthorized();

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
            return Unauthorized();

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

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return Json(new { success = false, message = "Invalid file type" });

        // Validate file size (5MB)
        if (file.Length > 5 * 1024 * 1024)
            return Json(new { success = false, message = "File size exceeds 5MB" });

        try
        {
            // Generate unique filename
            var fileName = $"course-thumbnail-{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "thumbnails");
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(uploadsFolder);
            
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            // Return URL
            var fileUrl = $"/uploads/thumbnails/{fileName}";
            return Json(new { success = true, url = fileUrl });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Upload failed: " + ex.Message });
        }
    }
}
