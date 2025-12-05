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
    private readonly AppDbContext _context;

    public InstructorCoursesService(
        ICourseRepository courseRepo, 
        ICategoryRepository categoryRepo,
        AppDbContext context)
    {
        _courseRepo = courseRepo;
        _context = context;
    }

    public async Task<PagedResultDto<InstructorCourseDto>> GetInstructorCoursesPagedAsync(int instructorId, PagingRequestDto request)
    {
        var query = _courseRepo.GetCoursesByInstructorQueryable(instructorId);

        bool isASC = request.SortOrder == SortOrder.Ascending;
        CourseSortOption sortBy = request.SortBy;

        IQueryable<Course> sortedQuery = sortBy switch
        {
            CourseSortOption.CreationDate => isASC ? query.OrderBy(c => c.CreatedDate)
                                                   : query.OrderByDescending(c => c.CreatedDate),

            CourseSortOption.Popularity => isASC ? query.OrderBy(c => c.Enrollments!.Count())
                                                 : query.OrderByDescending(c => c.Enrollments!.Count()),

            CourseSortOption.Title => isASC ? query.OrderBy(c => c.Title)
                                            : query.OrderByDescending(c => c.Title),

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
                Status = c.Status.ToString(),
                Level = c.Level,
                MainCategory = c.Categories.Select(cat => cat.Name).FirstOrDefault(),

                // calculated fields
                NumberOfModules = c.Modules.Count(),
                NumberOfStudents = c.Enrollments!.Count(),
                NumberOfMinutes = (int)(
                                c.Modules
                                    .SelectMany(m => m.Lessons)
                                    .Select(l => l.LessonContent)
                                    .OfType<VideoContent>()
                                    .Sum(l => l.DurationInSeconds) / 60.0),
                AverageRating = 4.7f
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
    public async Task<InstructorCourseEditDto?> GetCourseForEditAsync(int instructorId, int courseId)
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
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync();

        if (course == null)
            return null;

        // Calculate stats
        var enrollments = course.Enrollments ?? new List<CourseEnrollment>();
        var completedCount = enrollments.Count(e => e.ProgressPercentage >= 100);
        var avgProgress = enrollments.Any() 
            ? (int)enrollments.Average(e => (float)e.ProgressPercentage) 
            : 0;

        return new InstructorCourseEditDto
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
                            ArticleContent = l.LessonContent is ArticleContent pdf ? pdf.Content : null,
                            Resources = l.LessonResources?
                                .Select(r => new LessonResourceEditDto
                                {
                                    LessonResourceId = r.LessonResourceId,
                                    ResourceType = r.ResourceKind,
                                    Url = r.Url,
                                    Title = r.Title,
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
            Console.WriteLine($"🔍 UpdateCourseAsync called for courseId: {courseId}");
            
            // Load the course WITH tracking enabled
            var course = await _context.Courses
                .Where(c => c.Id == courseId && c.InstructorId == instructorId)
                .FirstOrDefaultAsync();

            if (course == null)
            {
                Console.WriteLine($"❌ Course not found");
                return false;
            }

            // Update only the specific properties (EF Core tracks changes automatically)
            course.Title = updateDto.Title?.Trim() ?? course.Title;
            course.Description = updateDto.Description ?? course.Description;
            course.Level = updateDto.Level;
            course.ThumbnailImageUrl = updateDto.ThumbnailUrl ?? course.ThumbnailImageUrl;

            // Remove existing learning outcomes using raw SQL (composite key issue)
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM CourseLearningOutcomes WHERE course_id = {0}",
                courseId
            );

            // Add new learning outcomes - need to get max Id for the course to avoid PK conflicts
            if (updateDto.LearningOutcomes != null && updateDto.LearningOutcomes.Any())
            {
                // Get the max Id from the table to generate new Ids manually
                var maxId = await _context.CourseLearningOutcomes
                    .MaxAsync(o => (int?)o.Id) ?? 0;

                int newId = maxId + 1;
                foreach (var outcomeTitle in updateDto.LearningOutcomes.Where(o => !string.IsNullOrWhiteSpace(o)))
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO CourseLearningOutcomes (course_outcome_id, title, description, course_id) VALUES ({0}, {1}, {2}, {3})",
                        newId++, outcomeTitle.Trim(), outcomeTitle.Trim(), courseId
                    );
                }
            }

            // Handle category using raw SQL to avoid tracking issues
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM LearningEntity_Category WHERE learning_entity_id = {0}",
                courseId
            );

            if (updateDto.CategoryId > 0)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO LearningEntity_Category (learning_entity_id, category_id) VALUES ({0}, {1})",
                    courseId, updateDto.CategoryId
                );
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ SUCCESS!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: {ex.Message}");
            Console.WriteLine($"❌ INNER: {ex.InnerException?.Message}");
            Console.WriteLine($"❌ STACK: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<UpdateResultDto> UpdateModuleAsync(int instructorId, int courseId, UpdateModuleDto moduleDto)
    {
        try
        {
            // Verify course ownership
            var courseExists = await _context.Courses
                .AnyAsync(c => c.Id == courseId && c.InstructorId == instructorId);

            if (!courseExists)
            {
                Console.WriteLine($"❌ Course {courseId} not found for instructor {instructorId}");
                return new UpdateResultDto { Success = false, Message = "Course not found" };
            }

            int moduleId;

            if (moduleDto.ModuleId.HasValue)
            {
                // Update existing module
                var module = await _context.Modules
                    .FirstOrDefaultAsync(m => m.ModuleId == moduleDto.ModuleId.Value && m.CourseId == courseId);

                if (module == null)
                {
                    Console.WriteLine($"❌ Module {moduleDto.ModuleId} not found");
                    return new UpdateResultDto { Success = false, Message = "Module not found" };
                }

                module.Title = moduleDto.Title;
                module.Description = moduleDto.Description;
                module.Order = moduleDto.Order;
                moduleId = module.ModuleId;
            }
            else
            {
                // Create new module - get the next available order
                var maxOrder = await _context.Modules
                    .Where(m => m.CourseId == courseId)
                    .MaxAsync(m => (int?)m.Order) ?? 0;

                var newModule = new Module
                {
                    CourseId = courseId,
                    Title = moduleDto.Title,
                    Description = moduleDto.Description,
                    Order = maxOrder + 1
                };

                await _context.Modules.AddAsync(newModule);
                await _context.SaveChangesAsync();
                moduleId = newModule.ModuleId;
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Module saved successfully with ID: {moduleId}");
            return new UpdateResultDto { Success = true, ModuleId = moduleId };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error updating module: {ex.Message}");
            Console.WriteLine($"❌ Inner: {ex.InnerException?.Message}");
            return new UpdateResultDto { Success = false, Message = ex.Message };
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

    public async Task<UpdateResultDto> UpdateLessonAsync(int instructorId, int courseId, UpdateLessonDto lessonDto)
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
                return new UpdateResultDto { Success = false, Message = "Module not found or access denied" };

            int lessonId;

            if (lessonDto.LessonId.HasValue)
            {
                // Update existing lesson
                var lesson = await _context.Lessons
                    .Include(l => l.LessonContent)
                    .Include(l => l.LessonResources)
                    .FirstOrDefaultAsync(l => l.LessonId == lessonDto.LessonId.Value && 
                                     l.ModuleId == lessonDto.ModuleId);

                if (lesson == null)
                    return new UpdateResultDto { Success = false, Message = "Lesson not found" };

                lesson.Title = lessonDto.Title;
                lesson.ContentType = lessonDto.ContentType;
                lesson.Order = lessonDto.Order;
                lessonId = lesson.LessonId;

                // Update lesson content
                if (lesson.LessonContent != null)
                {
                    if (lessonDto.ContentType == LessonContentType.Video && lesson.LessonContent is VideoContent videoContent)
                    {
                        videoContent.VideoUrl = lessonDto.VideoUrl ?? videoContent.VideoUrl;
                        videoContent.DurationInSeconds = lessonDto.DurationInSeconds ?? videoContent.DurationInSeconds;
                    }
                    else if (lessonDto.ContentType == LessonContentType.Article && lesson.LessonContent is ArticleContent pdfContent)
                    {
                        pdfContent.Content = lessonDto.PdfUrl ?? pdfContent.Content;
                    }
                }

                // UPDATE RESOURCES
                var resources = lessonDto.Resources ?? new List<UpdateLessonResourceDto>();
                
                if (lesson.LessonResources != null)
                {
                    var resourcesToRemove = lesson.LessonResources
                        .Where(r => !resources.Any(ur => ur.LessonResourceId == r.LessonResourceId))
                        .ToList();
                    
                    foreach (var resource in resourcesToRemove)
                    {
                        _context.LessonResources.Remove(resource);
                    }

                    foreach (var resourceDto in resources.Where(r => r.LessonResourceId.HasValue))
                    {
                        var existingResource = lesson.LessonResources
                            .FirstOrDefault(r => r.LessonResourceId == resourceDto.LessonResourceId.Value);
                        
                        if (existingResource != null)
                        {
                            existingResource.Url = resourceDto.Url;
                            existingResource.Title = resourceDto.Title ?? existingResource.Title;
                        }
                    }
                }

                // Add new resources
                foreach (var resourceDto in resources.Where(r => !r.LessonResourceId.HasValue))
                {
                    LessonResource newResource = resourceDto.ResourceType switch
                    {
                        LessonResourceType.PDF => new PdfResource
                        {
                            LessonId = lesson.LessonId,
                            Url = resourceDto.Url,
                            Title = resourceDto.Title ?? "PDF Resource"
                        },
                        LessonResourceType.URL => new UrlResource
                        {
                            LessonId = lesson.LessonId,
                            Url = resourceDto.Url,
                            Title = resourceDto.Title ?? "URL Resource"
                        },
                        LessonResourceType.ZIP => new ZipResource
                        {
                            LessonId = lesson.LessonId,
                            Url = resourceDto.Url,
                            Title = resourceDto.Title ?? "ZIP Resource"
                        },
                        _ => throw new ArgumentException("Invalid resource type")
                    };

                    await _context.LessonResources.AddAsync(newResource);
                }
            }
            else
            {
                // Create new lesson
                var maxOrder = await _context.Lessons
                    .Where(l => l.ModuleId == lessonDto.ModuleId)
                    .MaxAsync(l => (int?)l.Order) ?? 0;

                var newLesson = new Lesson
                {
                    ModuleId = lessonDto.ModuleId,
                    Title = lessonDto.Title,
                    ContentType = lessonDto.ContentType,
                    Order = maxOrder + 1
                };

                await _context.Lessons.AddAsync(newLesson);
                await _context.SaveChangesAsync();
                lessonId = newLesson.LessonId;

                // Create lesson content
                LessonContent content;
                if (lessonDto.ContentType == LessonContentType.Video)
                {
                    content = new VideoContent
                    {
                        LessonId = newLesson.LessonId,
                        VideoUrl = lessonDto.VideoUrl ?? string.Empty,
                        DurationInSeconds = lessonDto.DurationInSeconds ?? 0
                    };
                }
                else
                {
                    content = new ArticleContent
                    {
                        LessonId = newLesson.LessonId,
                        Content = lessonDto.PdfUrl ?? lessonDto.Title
                    };
                }

                await _context.LessonContents.AddAsync(content);

                // Add resources for new lesson
                var resources = lessonDto.Resources ?? new List<UpdateLessonResourceDto>();
                foreach (var resourceDto in resources)
                {
                    LessonResource newResource = resourceDto.ResourceType switch
                    {
                        LessonResourceType.PDF => new PdfResource
                        {
                            LessonId = newLesson.LessonId,
                            Url = resourceDto.Url,
                            Title = resourceDto.Title ?? "PDF Resource"
                        },
                        LessonResourceType.URL => new UrlResource
                        {
                            LessonId = newLesson.LessonId,
                            Url = resourceDto.Url,
                            Title = resourceDto.Title ?? "URL Resource"
                        },
                        LessonResourceType.ZIP => new ZipResource
                        {
                            LessonId = newLesson.LessonId,
                            Url = resourceDto.Url,
                            Title = resourceDto.Title ?? "ZIP Resource"
                        },
                        _ => throw new ArgumentException("Invalid resource type")
                    };

                    await _context.LessonResources.AddAsync(newResource);
                }
            }

            await _context.SaveChangesAsync();
            return new UpdateResultDto { Success = true, LessonId = lessonId };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating lesson: {ex.Message}");
            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            return new UpdateResultDto { Success = false, Message = ex.Message };
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
