using BLL.DTOs.Course;
using BLL.Interfaces;
using Core.Entities;
using Core.Entities.Enums;
using Core.RepositoryInterfaces;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services
{
    public class CourseCreationService : ICourseCreationService
    {
        private readonly AppDbContext _context;
        private readonly ICategoryRepository _categoryRepo;
        private readonly ILanguageRepository _languageRepo;

        public CourseCreationService(
            AppDbContext context,
            ICategoryRepository categoryRepo,
            ILanguageRepository languageRepo)
        {
            _context = context;
            _categoryRepo = categoryRepo;
            _languageRepo = languageRepo;
        }

        public async Task<CourseCreationResultDto> CreateCourseAsync(CreateCourseDto dto)
        {
            try
            {
                Console.WriteLine("?? Starting course creation...");
                Console.WriteLine($"?? Instructor ID: {dto.InstructorId}");
                Console.WriteLine($"?? Title: {dto.Title}");

                // 1. Create the course
                var course = new Course
                {
                    InstructorId = dto.InstructorId,
                    Title = dto.Title,
                    Description = dto.Description,
                    ThumbnailImageUrl = dto.ThumbnailImageUrl,
                    Level = dto.Level,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                Console.WriteLine($"? Course created with ID: {course.Id}");

                // 2. Add category using navigation properties
                if (dto.CategoryId > 0)
                {
                    var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
                    if (category != null)
                    {
                        var courseWithNav = await _context.Courses
                            .Include(c => c.Categories)
                            .FirstOrDefaultAsync(c => c.Id == course.Id);
                        
                        if (courseWithNav != null)
                        {
                            if (courseWithNav.Categories == null)
                                courseWithNav.Categories = new List<Category>();
                                
                            courseWithNav.Categories.Add(category);
                            await _context.SaveChangesAsync();
                            Console.WriteLine($"? Category added: {category.Name}");
                        }
                    }
                }

                // 3. Add language using navigation properties
                if (dto.LanguageId > 0)
                {
                    var language = await _languageRepo.GetByIdAsync(dto.LanguageId);
                    if (language != null)
                    {
                        var courseWithNav = await _context.Courses
                            .Include(c => c.Languages)
                            .FirstOrDefaultAsync(c => c.Id == course.Id);
                        
                        if (courseWithNav != null)
                        {
                            if (courseWithNav.Languages == null)
                                courseWithNav.Languages = new List<Language>();
                                
                            courseWithNav.Languages.Add(language);
                            await _context.SaveChangesAsync();
                            Console.WriteLine($"? Language added: {language.Name}");
                        }
                    }
                }

                // 4. Add learning outcomes
                if (dto.LearningOutcomes != null && dto.LearningOutcomes.Any())
                {
                    for (int i = 0; i < dto.LearningOutcomes.Count; i++)
                    {
                        var outcome = new CourseLearningOutcome
                        {
                            Id = i + 1,
                            CourseId = course.Id,
                            Title = dto.LearningOutcomes[i],
                            Description = dto.LearningOutcomes[i]
                        };
                        _context.CourseLearningOutcomes.Add(outcome);
                    }
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"? Added {dto.LearningOutcomes.Count} learning outcomes");
                }

                // 5. Add modules, lessons, and content
                if (dto.Modules != null && dto.Modules.Any())
                {
                    foreach (var moduleDto in dto.Modules)
                    {
                        var module = new Module
                        {
                            CourseId = course.Id,
                            Title = moduleDto.Title,
                            Description = moduleDto.Description,
                            Order = moduleDto.Order
                        };

                        _context.Modules.Add(module);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"? Module created: {module.Title}");

                        // Add lessons
                        if (moduleDto.Lessons != null && moduleDto.Lessons.Any())
                        {
                            foreach (var lessonDto in moduleDto.Lessons)
                            {
                                var lesson = new Lesson
                                {
                                    ModuleId = module.ModuleId,
                                    Title = lessonDto.Title,
                                    ContentType = lessonDto.ContentType,
                                    Order = lessonDto.Order
                                };

                                _context.Lessons.Add(lesson);
                                await _context.SaveChangesAsync();
                                Console.WriteLine($"  ? Lesson created: {lesson.Title}");

                                // Add lesson content
                                LessonContent content;
                                if (lessonDto.ContentType == LessonContentType.Video)
                                {
                                    content = new VideoContent
                                    {
                                        LessonId = lesson.LessonId,
                                        VideoUrl = lessonDto.VideoUrl ?? "",
                                        DurationInSeconds = lessonDto.DurationInSeconds ?? 0
                                    };
                                }
                                else
                                {
                                    // Article content - use PdfUrl as the article content/HTML
                                    content = new ArticleContent
                                    {
                                        LessonId = lesson.LessonId,
                                        Content = lessonDto.PdfUrl ?? "Article content goes here..."
                                    };
                                }

                                _context.LessonContents.Add(content);
                                await _context.SaveChangesAsync();
                                Console.WriteLine($"    ? Lesson content added");

                                // Add lesson resources
                                if (lessonDto.Resources != null && lessonDto.Resources.Any())
                                {
                                    foreach (var resourceDto in lessonDto.Resources)
                                    {
                                        LessonResource resource = resourceDto.ResourceType switch
                                        {
                                            LessonResourceType.PDF => new PdfResource
                                            {
                                                LessonId = lesson.LessonId,
                                                Url = resourceDto.Url,
                                                Title = resourceDto.Title ?? "PDF Resource"
                                            },
                                            LessonResourceType.ZIP => new ZipResource
                                            {
                                                LessonId = lesson.LessonId,
                                                Url = resourceDto.Url,
                                                Title = resourceDto.Title ?? "ZIP Resource"
                                            },
                                            LessonResourceType.URL => new UrlResource
                                            {
                                                LessonId = lesson.LessonId,
                                                Url = resourceDto.Url,
                                                Title = resourceDto.Title ?? "URL Resource"
                                            },
                                            _ => new UrlResource
                                            {
                                                LessonId = lesson.LessonId,
                                                Url = resourceDto.Url,
                                                Title = resourceDto.Title ?? "Resource"
                                            }
                                        };

                                        _context.LessonResources.Add(resource);
                                        Console.WriteLine($"    ?? Resource added: {resourceDto.ResourceType} - {resourceDto.Url}");
                                    }
                                    await _context.SaveChangesAsync();
                                    Console.WriteLine($"    ? Added {lessonDto.Resources.Count} resources to lesson");
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"?? Course published successfully! CourseId: {course.Id}");

                return new CourseCreationResultDto
                {
                    Success = true,
                    CourseId = course.Id
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error creating course: {ex.Message}");
                Console.WriteLine($"? Exception type: {ex.GetType().Name}");
                Console.WriteLine($"? Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"? Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"? Inner stack trace: {ex.InnerException.StackTrace}");
                }
                
                return new CourseCreationResultDto
                {
                    Success = false,
                    ErrorMessage = $"Failed to create course: {ex.Message}"
                };
            }
        }
    }
}