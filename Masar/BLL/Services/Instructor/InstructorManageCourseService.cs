using BLL.DTOs.Instructor.ManageCourse;
using BLL.Interfaces.Instructor;
using Core.Entities;
using Core.Entities.Enums;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Instructor;

public class InstructorManageCourseService : IInstructorManageCourseService
{
    private readonly ICourseRepository _courseRepo;
    private readonly IInstructorProfileRepository _instructorProfileRepo;
    private readonly ILessonRepository _lessonRepo;
    private readonly AppDbContext _context;

    public InstructorManageCourseService(
        ICourseRepository courseRepository,
        IInstructorProfileRepository instructorProfileRepository,
        ILessonRepository lessonRepository,
        AppDbContext context)
    {
        _courseRepo = courseRepository;
        _instructorProfileRepo = instructorProfileRepository;
        _lessonRepo = lessonRepository;
        _context = context;
    }

    public async Task<ManageViewCourseDto?> GetCourseForManageAsync(int instructorId, int courseId)
    {
        var courseQuery = _courseRepo.GetCourseByIdQueryable(courseId)
            .Where(c => c.InstructorId == instructorId);

        if (!await courseQuery.AnyAsync())
            return null;

        var courseManageData = await courseQuery
            .AsSplitQuery()
            .Select(c => new ManageViewCourseDto
            {
                CourseId = c.Id,
                CourseTitle = c.Title,
                CourseStatus = c.Status,
                Curriculum = new ManageViewCourseCurriculumDto
                {
                    CourseId = c.Id,
                    CourseTitle = c.Title,
                    CourseStatus = c.Status,

                    Modules = c.Modules.OrderBy(m => m.Order).Select(m => new ManageViewModuleDto
                    {
                        ModuleId = m.ModuleId,
                        ModuleTitle = m.Title,
                        ModuleOrder = m.Order,
                        LessonsCount = m.Lessons.Count(),

                        Lessons = m.Lessons.OrderBy(l => l.Order).Select(l => new ManageViewLessonDto
                        {
                            LessonId = l.LessonId,
                            LessonTitle = l.Title,
                            LessonOrder = l.Order,
                            ContentType = l.ContentType,

                            DurationInMinutes = (l.LessonContent is VideoContent) ? (int)Math.Round(((VideoContent)l.LessonContent).DurationInSeconds / 60.0) : null
                        })
                    })
                }
            })
            .FirstOrDefaultAsync();

        if (courseManageData?.Curriculum?.Modules != null)
        {
            foreach (var module in courseManageData.Curriculum.Modules)
            {
                var videoLessonsDurations = module.Lessons
                    .Select(l => l.DurationInMinutes)
                    .Where(duration => duration != null);

                module.DurationInMinutes = videoLessonsDurations.Any() ? videoLessonsDurations.Sum() : null;
            }
        }

        return courseManageData;
    }

    public async Task<ManageViewCourseCurriculumDto?> GetCourseCurriculumAsync(int instructorId, int courseId)
    {
        var courseQuery = _courseRepo.GetCourseByIdQueryable(courseId)
            .Where(c => c.InstructorId == instructorId);

        if (!await courseQuery.AnyAsync())
            return null;

        var courseCurriculumDto = await courseQuery.Select(c => new ManageViewCourseCurriculumDto
        {
            CourseId = c.Id,
            CourseTitle = c.Title,
            CourseStatus = c.Status,

            Modules = c.Modules.OrderBy(m => m.Order).Select(m => new ManageViewModuleDto
            {
                ModuleId = m.ModuleId,
                ModuleTitle = m.Title,
                ModuleOrder = m.Order,
                LessonsCount = m.Lessons.Count(),

                Lessons = m.Lessons.OrderBy(l => l.Order).Select(l => new ManageViewLessonDto
                {
                    LessonId = l.LessonId,
                    LessonTitle = l.Title,
                    LessonOrder = l.Order,
                    ContentType = l.ContentType,

                    DurationInMinutes = (l.LessonContent is VideoContent) ? (int)Math.Round(((VideoContent)l.LessonContent).DurationInSeconds / 60.0) : null
                })
            })
        })
        .FirstOrDefaultAsync();

        return courseCurriculumDto;
    }

    public async Task<ManageEditLessonDto?> GetLessonDataAsync(int instructorId, int lessonId)
    {
        var lessonDataDto = await _lessonRepo.GetByIdQueryable(lessonId)
            .Where(l => l.Module.Course.InstructorId == instructorId)
            .Select(l => new ManageEditLessonDto
            {
                LessonId = l.LessonId,
                LessonTitle = l.Title,
                LessonOrder = l.Order,

                ContentType = l.ContentType,

                DurationInMinutes = (l.LessonContent is VideoContent)
                    ? (int?)Math.Round(((VideoContent)l.LessonContent).DurationInSeconds / 60.0)
                    : null,

                VideoUrl = (l.LessonContent is VideoContent)
                    ? ((VideoContent)l.LessonContent).VideoUrl
                    : null,

                ArticleContent = (l.LessonContent is ArticleContent)
                    ? ((ArticleContent)l.LessonContent).Content
                    : null,

                Resources = l.LessonResources.Select(r => new ManageEditLessonResourceDto
                {
                    LessonResourceId = r.LessonResourceId,
                    ResourceType = r.ResourceKind,
                    Title = r.Title,
                    Url = r.Url
                })
            })
            .FirstOrDefaultAsync();

        return lessonDataDto;
    }

    public async Task<ManageSaveResultDto> SaveModuleAsync(int instructorId, int courseId, ManageSaveModuleDto moduleDto)
    {
        try
        {
            // Verify course ownership
            var courseExists = await _context.Courses
                .AnyAsync(c => c.Id == courseId && c.InstructorId == instructorId);

            if (!courseExists)
                return new ManageSaveResultDto { Success = false, Message = "Course not found or access denied" };

            int moduleId;

            if (moduleDto.ModuleId > 0)
            {
                // Update existing module
                var module = await _context.Modules
                    .FirstOrDefaultAsync(m => m.ModuleId == moduleDto.ModuleId && m.CourseId == courseId);

                if (module == null)
                    return new ManageSaveResultDto { Success = false, Message = "Module not found" };

                module.Title = moduleDto.ModuleTitle;
                moduleId = module.ModuleId;
                
                // Save the update
                await _context.SaveChangesAsync();
            }
            else
            {
                // Create new module
                var maxOrder = await _context.Modules
                    .Where(m => m.CourseId == courseId)
                    .MaxAsync(m => (int?)m.Order) ?? 0;

                var newModule = new Module
                {
                    CourseId = courseId,
                    Title = moduleDto.ModuleTitle,
                    Order = maxOrder + 1
                };

                await _context.Modules.AddAsync(newModule);
                await _context.SaveChangesAsync();
                moduleId = newModule.ModuleId;
            }

            return new ManageSaveResultDto { Success = true, EntityId = moduleId };
        }
        catch (DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            return new ManageSaveResultDto { Success = false, Message = $"Database error: {innerMessage}" };
        }
        catch (Exception ex)
        {
            return new ManageSaveResultDto { Success = false, Message = ex.Message };
        }
    }

    public async Task<bool> DeleteModuleAsync(int instructorId, int courseId, int moduleId)
    {
        try
        {
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
        catch
        {
            return false;
        }
    }

    public async Task<ManageSaveResultDto> SaveLessonAsync(int instructorId, int courseId, ManageSaveLessonDto lessonDto)
    {
        try
        {
            // Verify module belongs to instructor's course
            var moduleExists = await _context.Modules
                .Include(m => m.Course)
                .AnyAsync(m => m.ModuleId == lessonDto.ModuleId &&
                              m.CourseId == courseId &&
                              m.Course!.InstructorId == instructorId);

            if (!moduleExists)
                return new ManageSaveResultDto { Success = false, Message = "Module not found or access denied" };

            var contentType = lessonDto.ContentType == "Video" ? LessonContentType.Video : LessonContentType.Article;
            int lessonId;

            if (lessonDto.LessonId > 0)
            {
                // Update existing lesson
                var lesson = await _context.Lessons
                    .Include(l => l.LessonContent)
                    .Include(l => l.LessonResources)
                    .FirstOrDefaultAsync(l => l.LessonId == lessonDto.LessonId && l.ModuleId == lessonDto.ModuleId);

                if (lesson == null)
                    return new ManageSaveResultDto { Success = false, Message = "Lesson not found" };

                lesson.Title = lessonDto.Title;
                lesson.ContentType = contentType;
                lessonId = lesson.LessonId;

                // Update or replace content
                if (lesson.LessonContent != null)
                {
                    _context.LessonContents.Remove(lesson.LessonContent);
                }

                await _context.SaveChangesAsync();

                // Add new content
                if (contentType == LessonContentType.Video)
                {
                    var videoContent = new VideoContent
                    {
                        LessonId = lesson.LessonId,
                        VideoUrl = lessonDto.VideoUrl ?? "",
                        DurationInSeconds = (lessonDto.DurationMinutes ?? 0) * 60
                    };
                    await _context.LessonContents.AddAsync(videoContent);
                }
                else
                {
                    var articleContent = new ArticleContent
                    {
                        LessonId = lesson.LessonId,
                        Content = lessonDto.ArticleContent ?? ""
                    };
                    await _context.LessonContents.AddAsync(articleContent);
                }

                // Update resources
                if (lesson.LessonResources != null)
                {
                    _context.LessonResources.RemoveRange(lesson.LessonResources);
                }

                await _context.SaveChangesAsync();

                // Add new resources
                if (lessonDto.Resources != null)
                {
                    foreach (var resDto in lessonDto.Resources)
                    {
                        var resource = CreateLessonResource(lesson.LessonId, resDto);
                        await _context.LessonResources.AddAsync(resource);
                    }
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
                    ContentType = contentType,
                    Order = maxOrder + 1
                };

                await _context.Lessons.AddAsync(newLesson);
                await _context.SaveChangesAsync();
                lessonId = newLesson.LessonId;

                // Add content
                if (contentType == LessonContentType.Video)
                {
                    var videoContent = new VideoContent
                    {
                        LessonId = newLesson.LessonId,
                        VideoUrl = lessonDto.VideoUrl ?? "",
                        DurationInSeconds = (lessonDto.DurationMinutes ?? 0) * 60
                    };
                    await _context.LessonContents.AddAsync(videoContent);
                }
                else
                {
                    var articleContent = new ArticleContent
                    {
                        LessonId = newLesson.LessonId,
                        Content = lessonDto.ArticleContent ?? ""
                    };
                    await _context.LessonContents.AddAsync(articleContent);
                }

                // Add resources
                if (lessonDto.Resources != null)
                {
                    foreach (var resDto in lessonDto.Resources)
                    {
                        var resource = CreateLessonResource(newLesson.LessonId, resDto);
                        await _context.LessonResources.AddAsync(resource);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return new ManageSaveResultDto { Success = true, EntityId = lessonId };
        }
        catch (Exception ex)
        {
            return new ManageSaveResultDto { Success = false, Message = ex.Message };
        }
    }

    public async Task<bool> DeleteLessonAsync(int instructorId, int courseId, int lessonId)
    {
        try
        {
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
        catch
        {
            return false;
        }
    }

    public async Task<ManageSaveResultDto> SaveBasicInfoAsync(int instructorId, int courseId, ManageSaveBasicInfoDto basicInfoDto)
    {
        try
        {
            // Verify course ownership
            var course = await _context.Courses
                .Include(c => c.Categories)
                .Include(c => c.Languages)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

            if (course == null)
                return new ManageSaveResultDto { Success = false, Message = "Course not found or access denied" };

            // Update basic fields
            course.Title = basicInfoDto.CourseTitle.Trim();
            course.Description = basicInfoDto.CourseDescription.Trim();

            // Update thumbnail if provided
            if (!string.IsNullOrEmpty(basicInfoDto.ThumbnailImageUrl))
            {
                course.ThumbnailImageUrl = basicInfoDto.ThumbnailImageUrl;
            }

            // Parse and update level
            if (Enum.TryParse<CourseLevel>(basicInfoDto.Level, true, out var level))
            {
                course.Level = level;
            }

            // Update categories - clear existing and add new
            course.Categories?.Clear();
            if (basicInfoDto.CategoryIds != null && basicInfoDto.CategoryIds.Any())
            {
                var categories = await _context.Categories
                    .Where(c => basicInfoDto.CategoryIds.Contains(c.CategoryId))
                    .ToListAsync();

                course.Categories = categories;
            }

            // Update languages - clear existing and add new
            course.Languages?.Clear();
            if (basicInfoDto.LanguageIds != null && basicInfoDto.LanguageIds.Any())
            {
                var languages = await _context.Languages
                    .Where(l => basicInfoDto.LanguageIds.Contains(l.LanguageId))
                    .ToListAsync();

                course.Languages = languages;
            }

            await _context.SaveChangesAsync();

            return new ManageSaveResultDto { Success = true, EntityId = courseId, Message = "Basic info saved successfully" };
        }
        catch (Exception ex)
        {
            return new ManageSaveResultDto { Success = false, Message = ex.Message };
        }
    }

    public async Task<ManageSaveResultDto> SaveLearningOutcomeAsync(int instructorId, int courseId, ManageSaveLearningOutcomeDto outcomeDto)
    {
        try
        {
            // Verify course ownership
            var courseExists = await _context.Courses
                .AnyAsync(c => c.Id == courseId && c.InstructorId == instructorId);

            if (!courseExists)
                return new ManageSaveResultDto { Success = false, Message = "Course not found or access denied" };

            int outcomeId;

            if (outcomeDto.LearningOutcomeId > 0)
            {
                // Update existing outcome
                var outcome = await _context.CourseLearningOutcomes
                    .FirstOrDefaultAsync(o => o.Id == outcomeDto.LearningOutcomeId && o.CourseId == courseId);

                if (outcome == null)
                    return new ManageSaveResultDto { Success = false, Message = "Learning outcome not found" };

                outcome.Title = outcomeDto.OutcomeName.Trim();
                outcomeId = outcome.Id;
            }
            else
            {
                // Create new outcome
                var newOutcome = new CourseLearningOutcome
                {
                    CourseId = courseId,
                    Title = outcomeDto.OutcomeName.Trim(),
                    Description = "" // Ensure Description is not null
                };

                await _context.CourseLearningOutcomes.AddAsync(newOutcome);
                await _context.SaveChangesAsync();
                outcomeId = newOutcome.Id;
                
                // Return early for new outcomes - already saved
                return new ManageSaveResultDto { Success = true, EntityId = outcomeId, Message = "Learning outcome saved successfully" };
            }

            // Only save here for updates
            await _context.SaveChangesAsync();
            return new ManageSaveResultDto { Success = true, EntityId = outcomeId, Message = "Learning outcome saved successfully" };
        }
        catch (DbUpdateException dbEx)
        {
            // Log the inner exception for debugging
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            return new ManageSaveResultDto { Success = false, Message = $"Database error: {innerMessage}" };
        }
        catch (Exception ex)
        {
            return new ManageSaveResultDto { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    public async Task<bool> DeleteLearningOutcomeAsync(int instructorId, int courseId, int outcomeId)
    {
        try
        {
            var outcome = await _context.CourseLearningOutcomes
                .Include(o => o.Course)
                .FirstOrDefaultAsync(o => o.Id == outcomeId &&
                                         o.CourseId == courseId &&
                                         o.Course!.InstructorId == instructorId);

            if (outcome == null)
                return false;

            _context.CourseLearningOutcomes.Remove(outcome);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private LessonResource CreateLessonResource(int lessonId, ManageSaveLessonResourceDto dto)
    {
        return dto.ResourceType switch
        {
            "PDF" => new PdfResource { LessonId = lessonId, Url = dto.Url, Title = dto.Title },
            "ZIP" => new ZipResource { LessonId = lessonId, Url = dto.Url, Title = dto.Title },
            _ => new UrlResource { LessonId = lessonId, Url = dto.Url, Title = dto.Title }
        };
    }
}
