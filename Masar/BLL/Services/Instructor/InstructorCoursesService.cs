using BLL.DTOs.Course;
using BLL.DTOs.Instructor;
using BLL.DTOs.Misc;
using BLL.Interfaces.Instructor;
using Core.Entities;
using Core.Entities.Enums;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using DAL.Data;

namespace BLL.Services.Instructor;

public class InstructorCoursesService : IInstructorCoursesService
{
    private readonly ICourseRepository _courseRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly AppDbContext _context;

    public InstructorCoursesService(
        ICourseRepository courseRepo, 
        ICategoryRepository categoryRepo,
        AppDbContext context)
    {
        _courseRepo = courseRepo;
        _categoryRepo = categoryRepo;
        _context = context;
    }

    public async Task<PagedResultDto<InstructorCourseDto>> GetInstructorCoursesPagedAsync(
        int instructorId,
        PagingRequestDto request)
    {
        var query = _courseRepo.GetCoursesByInstructorQueryable(instructorId);

        // Apply sorting BEFORE includes
        bool isASC = request.SortOrder.ToUpper() == "ASC";
        string sortBy = request.SortBy?.ToLower() ?? "createddate";

        IQueryable<Course> sortedQuery = sortBy switch
        {
            "createddate" => isASC ? query.OrderBy(c => c.CreatedDate)
                                   : query.OrderByDescending(c => c.CreatedDate),
            "students" => isASC ? query.OrderBy(c => c.Enrollments!.Count())
                                : query.OrderByDescending(c => c.Enrollments!.Count()),
            _ => query.OrderByDescending(c => c.CreatedDate)
        };

        // Now apply includes to the sorted query
        var finalQuery = sortedQuery
            .Include(c => c.Categories)
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
                    .ThenInclude(l => l.LessonContent)
            .Include(c => c.Enrollments);

        var totalCount = await finalQuery.CountAsync();

        var items = await finalQuery
            .Skip((request.CurrentPage - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new InstructorCourseDto
            {
                CourseId = c.Id,
                Title = c.Title,
                Description = c.Description,
                ThumbnailImageUrl = c.ThumbnailImageUrl,
                CreatedDate = c.CreatedDate,
                LastUpdatedDate = c.CreatedDate,
                Status = "Published",
                Level = c.Level,
                MainCategory = c.Categories != null && c.Categories.Any() 
                    ? c.Categories.FirstOrDefault() 
                    : null,
                NumberOfModules = c.Modules != null ? c.Modules.Count() : 0,
                NumberOfStudents = c.Enrollments != null ? c.Enrollments.Count() : 0,
                NumberOfMinutes = c.Modules != null 
                    ? (int)(c.Modules
                        .Where(m => m.Lessons != null)  // CHANGED: Use Where instead of SelectMany with ??
                        .SelectMany(m => m.Lessons!)
                        .Where(l => l.LessonContent != null)
                        .Select(l => l.LessonContent)
                        .OfType<VideoContent>()
                        .Sum(l => l.DurationInSeconds) / 60.0)
                    : 0,
                AverageRating = 4.8f
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResultDto<InstructorCourseDto>
        {
            Items = items ?? new List<InstructorCourseDto>(),
            Settings = new PaginationSettingsDto
            {
                TotalPages = totalPages,
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        };
    }

    // Update the GetCourseForEditAsync method to include the new fields
    public async Task<BLL.DTOs.Instructor.InstructorCourseEditDto?> GetCourseForEditAsync(int instructorId, int courseId)
    {
        var course = await _courseRepo.GetCourseByIdQueryable(courseId)
            .Where(c => c.InstructorId == instructorId)
            .Include(c => c.Categories)
            .Include(c => c.LearningOutcomes)
            .Include(c => c.Modules!)
                .ThenInclude(m => m.Lessons!)
                .ThenInclude(l => l.LessonContent)
            .Include(c => c.Modules!)
                .ThenInclude(m => m.Lessons!)
                .ThenInclude(l => l.LessonResources!)  // ADD THIS LINE
            .Include(c => c.Enrollments!)
                .ThenInclude(e => e.Student!)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync();

        if (course == null)
            return null;

        // Calculate stats
        var enrollments = course.Enrollments ?? new List<CourseEnrollment>();
        var completedCount = enrollments.Count(e => e.ProgressPercentage >= 100);
        var avgProgress = enrollments.Any() 
            ? (int)enrollments.Average(e => (float)e.ProgressPercentage) 
            : 0;

        return new BLL.DTOs.Instructor.InstructorCourseEditDto
        {
            CourseId = course.Id,
            Title = course.Title,
            Description = course.Description ?? string.Empty,
            ThumbnailImageUrl = course.ThumbnailImageUrl ?? string.Empty,
            Level = course.Level,
            MainCategory = course.Categories?.FirstOrDefault(),
            LearningOutcomes = course.LearningOutcomes ?? new List<CourseLearningOutcome>(),
            
            // Stats
            EnrolledStudents = enrollments.Count,
            AverageRating = 4.8f,
            Completions = completedCount,
            AverageProgress = avgProgress,
            
            // Modules
            Modules = course.Modules?
                .OrderBy(m => m.Order)
                .Select(m => new ModuleEditDto
                {
                    ModuleId = m.ModuleId,
                    Title = m.Title,
                    Description = m.Description,
                    Order = m.Order,
                    LessonsCount = m.Lessons?.Count ?? 0,
                    TotalDurationSeconds = m.Lessons?
                        .Select(l => l.LessonContent)
                        .OfType<VideoContent>()
                        .Sum(vc => vc.DurationInSeconds) ?? 0,
                    Lessons = m.Lessons?
                        .OrderBy(l => l.Order)
                        .Select(l => new LessonEditDto
                        {
                            LessonId = l.LessonId,
                            Title = l.Title,
                            Order = l.Order,
                            ContentType = l.ContentType,
                            DurationInSeconds = l.LessonContent is VideoContent vc 
                                ? vc.DurationInSeconds 
                                : 0,
                            VideoUrl = l.LessonContent is VideoContent video ? video.VideoUrl : null,
                            PdfUrl = l.LessonContent is PdfContent pdf ? pdf.PdfUrl : null,
                            Resources = l.LessonResources?
                                .Select(r => new LessonResourceEditDto
                                {
                                    LessonResourceId = r.LessonResourceId,
                                    ResourceType = r.ResourceKind,
                                    Url = r switch
                                    {
                                        PdfResource pdf => pdf.PdfUrl,
                                        UrlResource url => url.Link,
                                        ZipResource zip => zip.ZipUrl,
                                        _ => string.Empty
                                    },
                                    Title = r switch
                                    {
                                        PdfResource => "PDF Resource",
                                        UrlResource => "URL Resource",
                                        ZipResource => "ZIP Resource",
                                        _ => "Unknown"
                                    }
                                })
                                .ToList() ?? new List<LessonResourceEditDto>()
                        })
                        .ToList() ?? new List<LessonEditDto>()
                })
                .ToList() ?? new List<ModuleEditDto>(),
            
            // Enrolled Students (limit to recent 50 for performance)
            Students = enrollments
                .OrderByDescending(e => e.EnrollmentDate)
                .Take(50)
                .Select(e => new EnrolledStudentDto
                {
                    StudentId = e.StudentId,
                    FirstName = e.Student?.User?.FirstName ?? "Unknown",
                    LastName = e.Student?.User?.LastName ?? "User",
                    Email = e.Student?.User?.Email ?? string.Empty,
                    EnrollmentDate = e.EnrollmentDate.HasValue 
                        ? DateOnly.FromDateTime(e.EnrollmentDate.Value) 
                        : null,
                    ProgressPercentage = (float)e.ProgressPercentage,
                    LastAccessDate = e.EnrollmentDate // Using EnrollmentDate as fallback since LastAccessDate doesn't exist
                })
                .ToList()
        };
    }

    public async Task<bool> UpdateCourseAsync(int instructorId, int courseId, UpdateCourseDto updateDto)
    {
        try
        {
            // Get the course WITH TRACKING (don't use the repository's queryable method)
            var course = await _context.Courses
                .Where(c => c.Id == courseId && c.InstructorId == instructorId)
                .Include(c => c.LearningOutcomes)
                .Include(c => c.Categories)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                Console.WriteLine($"Course not found. CourseId: {courseId}, InstructorId: {instructorId}");
                return false;
            }

            Console.WriteLine($"Found course: {course.Title}");

            // Update basic properties
            course.Title = updateDto.Title;
            course.Description = updateDto.Description;
            course.Level = updateDto.Level;
            course.ThumbnailImageUrl = updateDto.ThumbnailUrl;

            Console.WriteLine("Updated basic properties");

            // Update learning outcomes - Remove existing ones
            if (course.LearningOutcomes != null && course.LearningOutcomes.Any())
            {
                var outcomesToRemove = course.LearningOutcomes.ToList();
                foreach (var outcome in outcomesToRemove)
                {
                    _context.CourseLearningOutcomes.Remove(outcome);
                }
                Console.WriteLine($"Removed {outcomesToRemove.Count} existing outcomes");
            }

            // Add new learning outcomes
            if (updateDto.LearningOutcomes != null && updateDto.LearningOutcomes.Any())
            {
                int outcomeId = 1;
                foreach (var outcomeTitle in updateDto.LearningOutcomes)
                {
                    var newOutcome = new CourseLearningOutcome
                    {
                        Id = outcomeId++,
                        Title = outcomeTitle,
                        Description = outcomeTitle,
                        CourseId = courseId
                    };
                    _context.CourseLearningOutcomes.Add(newOutcome);
                }
                Console.WriteLine($"Added {updateDto.LearningOutcomes.Count} new outcomes");
            }

            // Update categories - Remove all existing
            if (course.Categories != null && course.Categories.Any())
            {
                course.Categories.Clear();
                Console.WriteLine("Cleared existing categories");
            }

            // Add new category
            var category = await _context.Categories
                .Where(c => c.CategoryId == updateDto.CategoryId)
                .FirstOrDefaultAsync();
                
            if (category != null)
            {
                if (course.Categories == null)
                    course.Categories = new List<Category>();
                    
                course.Categories.Add(category);
                Console.WriteLine($"Added category: {category.Name}");
            }
            else
            {
                Console.WriteLine($"Category not found with ID: {updateDto.CategoryId}");
            }

            Console.WriteLine("About to save changes...");
            
            // Save changes to database
            var changeCount = await _context.SaveChangesAsync();
            
            Console.WriteLine($"Save completed. Changes saved: {changeCount}");
            
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"Database update error: {dbEx.Message}");
            Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
            Console.WriteLine($"Stack trace: {dbEx.StackTrace}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating course: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"Inner exception type: {ex.InnerException.GetType().Name}");
            }
            return false;
        }
    }

    public async Task<bool> UpdateModuleAsync(int instructorId, int courseId, UpdateModuleDto moduleDto)
    {
        try
        {
            // Verify course ownership
            var courseExists = await _context.Courses
                .AnyAsync(c => c.Id == courseId && c.InstructorId == instructorId);

            if (!courseExists)
                return false;

            if (moduleDto.ModuleId.HasValue)
            {
                // Update existing module
                var module = await _context.Modules
                    .FirstOrDefaultAsync(m => m.ModuleId == moduleDto.ModuleId.Value && m.CourseId == courseId);

                if (module == null)
                    return false;

                module.Title = moduleDto.Title;
                module.Description = moduleDto.Description;
                module.Order = moduleDto.Order;
            }
            else
            {
                // Create new module
                var newModule = new Module
                {
                    CourseId = courseId,
                    Title = moduleDto.Title,
                    Description = moduleDto.Description,
                    Order = moduleDto.Order
                };

                await _context.Modules.AddAsync(newModule);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating module: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteModuleAsync(int instructorId, int courseId, int moduleId)
    {
        try
        {
            // Verify course ownership
            var module = await _context.Modules
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.ModuleId == moduleId && 
                                         m.CourseId == courseId && 
                                         m.Course!.InstructorId == instructorId);

            if (module == null)
                return false;

            _context.Modules.Remove(module);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting module: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateLessonAsync(int instructorId, int courseId, UpdateLessonDto lessonDto)
    {
        try
        {
            // Verify course ownership and module belongs to course
            var moduleExists = await _context.Modules
                .Include(m => m.Course)
                .AnyAsync(m => m.ModuleId == lessonDto.ModuleId && 
                              m.CourseId == courseId && 
                              m.Course!.InstructorId == instructorId);

            if (!moduleExists)
                return false;

            if (lessonDto.LessonId.HasValue)
            {
                // Update existing lesson
                var lesson = await _context.Lessons
                    .Include(l => l.LessonContent)
                    .Include(l => l.LessonResources)  // ADD THIS
                    .FirstOrDefaultAsync(l => l.LessonId == lessonDto.LessonId.Value && 
                                             l.ModuleId == lessonDto.ModuleId);

                if (lesson == null)
                    return false;

                lesson.Title = lessonDto.Title;
                lesson.ContentType = lessonDto.ContentType;
                lesson.Order = lessonDto.Order;

                // Update lesson content
                if (lesson.LessonContent != null)
                {
                    if (lessonDto.ContentType == LessonContentType.Video && lesson.LessonContent is VideoContent videoContent)
                    {
                        videoContent.VideoUrl = lessonDto.VideoUrl ?? videoContent.VideoUrl;
                        videoContent.DurationInSeconds = lessonDto.DurationInSeconds ?? videoContent.DurationInSeconds;
                    }
                    else if (lessonDto.ContentType == LessonContentType.Article && lesson.LessonContent is PdfContent pdfContent)
                    {
                        pdfContent.PdfUrl = lessonDto.PdfUrl ?? pdfContent.PdfUrl;
                    }
                }

                // UPDATE RESOURCES
                if (lesson.LessonResources != null)
                {
                    // Remove resources that are not in the update list
                    var resourcesToRemove = lesson.LessonResources
                        .Where(r => !lessonDto.Resources.Any(ur => ur.LessonResourceId == r.LessonResourceId))
                        .ToList();
                    
                    foreach (var resource in resourcesToRemove)
                    {
                        _context.LessonResources.Remove(resource);
                    }

                    // Update existing resources
                    foreach (var resourceDto in lessonDto.Resources.Where(r => r.LessonResourceId.HasValue))
                    {
                        var existingResource = lesson.LessonResources
                            .FirstOrDefault(r => r.LessonResourceId == resourceDto.LessonResourceId.Value);
                        
                        if (existingResource != null)
                        {
                            // Update URL based on resource type
                            switch (existingResource)
                            {
                                case PdfResource pdfRes:
                                    pdfRes.PdfUrl = resourceDto.Url;
                                    break;
                                case UrlResource urlRes:
                                    urlRes.Link = resourceDto.Url;
                                    break;
                                case ZipResource zipRes:
                                    zipRes.ZipUrl = resourceDto.Url;
                                    break;
                            }
                        }
                    }
                }

                // Add new resources
                foreach (var resourceDto in lessonDto.Resources.Where(r => !r.LessonResourceId.HasValue))
                {
                    LessonResource newResource = resourceDto.ResourceType switch
                    {
                        LessonResourceType.PDF => new PdfResource
                        {
                            LessonId = lesson.LessonId,
                            PdfUrl = resourceDto.Url
                        },
                        LessonResourceType.URL => new UrlResource
                        {
                            LessonId = lesson.LessonId,
                            Link = resourceDto.Url
                        },
                        LessonResourceType.ZIP => new ZipResource
                        {
                            LessonId = lesson.LessonId,
                            ZipUrl = resourceDto.Url
                        },
                        _ => throw new ArgumentException("Invalid resource type")
                    };

                    await _context.LessonResources.AddAsync(newResource);
                }
            }
            else
            {
                // Create new lesson
                var newLesson = new Lesson
                {
                    ModuleId = lessonDto.ModuleId,
                    Title = lessonDto.Title,
                    ContentType = lessonDto.ContentType,
                    Order = lessonDto.Order
                };

                await _context.Lessons.AddAsync(newLesson);
                await _context.SaveChangesAsync(); // Save to get the LessonId

                // Create lesson content
                LessonContent content;
                if (lessonDto.ContentType == LessonContentType.Video)
                {
                    content = new VideoContent
                    {
                        LessonId = newLesson.LessonId,
                        Content = lessonDto.Title,
                        VideoUrl = lessonDto.VideoUrl ?? string.Empty,
                        DurationInSeconds = lessonDto.DurationInSeconds ?? 0
                    };
                }
                else
                {
                    content = new PdfContent
                    {
                        LessonId = newLesson.LessonId,
                        Content = lessonDto.Title,
                        PdfUrl = lessonDto.PdfUrl ?? string.Empty
                    };
                }

                await _context.LessonContents.AddAsync(content);

                // Add resources for new lesson
                foreach (var resourceDto in lessonDto.Resources)
                {
                    LessonResource newResource = resourceDto.ResourceType switch
                    {
                        LessonResourceType.PDF => new PdfResource
                        {
                            LessonId = newLesson.LessonId,
                            PdfUrl = resourceDto.Url
                        },
                        LessonResourceType.URL => new UrlResource
                        {
                            LessonId = newLesson.LessonId,
                            Link = resourceDto.Url
                        },
                        LessonResourceType.ZIP => new ZipResource
                        {
                            LessonId = newLesson.LessonId,
                            ZipUrl = resourceDto.Url
                        },
                        _ => throw new ArgumentException("Invalid resource type")
                    };

                    await _context.LessonResources.AddAsync(newResource);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating lesson: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<bool> DeleteLessonAsync(int instructorId, int courseId, int lessonId)
    {
        try
        {
            // Verify ownership
            var lesson = await _context.Lessons
                .Include(l => l.Module)
                    .ThenInclude(m => m!.Course)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId && 
                                         l.Module!.CourseId == courseId && 
                                         l.Module.Course!.InstructorId == instructorId);

            if (lesson == null)
                return false;

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting lesson: {ex.Message}");
            return false;
        }
    }
}
