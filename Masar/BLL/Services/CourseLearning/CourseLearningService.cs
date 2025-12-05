using BLL.DTOs.Classroom;
using BLL.Interfaces.CourseLearning;
using Core.Entities;
using Core.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.CourseLearning;

public class CourseLearningService : ICourseLearningService
{
    private readonly ICourseRepository _courseRepo;
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly ILessonProgressRepository _progressRepo;
    private readonly ILessonRepository _lessonRepo;
    private readonly IInstructorRepository _instructorRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CourseLearningService(
        ICourseRepository courseRepository, 
        IEnrollmentRepository enrollmentRepository, 
        ILessonProgressRepository lessonProgressRepository, 
        ILessonRepository lessonRepository,
        IInstructorRepository instructorRepository,
        IUnitOfWork unitOfWork)
    {
        _courseRepo = courseRepository;
        _enrollmentRepo = enrollmentRepository;
        _progressRepo = lessonProgressRepository;
        _lessonRepo = lessonRepository;
        _instructorRepo = instructorRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task<ClassroomDto?> GetClassroomAsync(int studentId, int courseId, int? lessonId = null)
    {
        // 1. Initial Checks & Queries 
        if (!await _courseRepo.HasCourseAsync(courseId))
            throw new ArgumentException($"No Course with Id: {courseId} found", nameof(courseId));

        var progressQuery = _progressRepo.GetAllLessonProgressForStudentQueryable(studentId);
        var enrollmentQuery = _enrollmentRepo.GetStudentCourseEnrollmentQueryable(studentId, courseId);


        // 2. Fetch the "Skeleton" (Structure only, no progress yet)
        var classroomDto = await enrollmentQuery
            .AsSplitQuery()
            .Select(e => new ClassroomDto
            {
                CourseId = e.CourseId,
                CourseTitle = e.Course!.Title,

                CourseProgress = new CourseProgressDto(),

                CourseOverviewStats = new CourseOverviewStatsDto
                {
                    CourseDescription = e.Course.Description,

                    TotalReviewsCount = 26534,
                    AverageRating = 3.6f,
                    Level = e.Course.Level.ToString(),

                    EnrolledStudentsCount = e.Course.Enrollments.Count(),

                    Categories = e.Course.Categories.Select(cat => cat.Name),
                    Languages = e.Course.Languages.Select(lang => lang.Name),
                    LearningOutcomes = e.Course.LearningOutcomes.Select(lo => lo.Title),

                    InstructorStats = new CourseInstructorStatsDto
                    {
                        InstructorId = e.Course.InstructorId,
                        InstructorName = e.Course.Instructor.User.FullName,
                        Bio = e.Course.Instructor.Bio,

                        ProfilePictureUrl = e.Course.Instructor.User.Picture,
                        YearsOfExperience = e.Course.Instructor.YearsOfExperience,
                        RoleTitle = "Senior Software Engineer @ Microsoft",
                        TotalCoursesCount = e.Course.Instructor.OwnedCourses.Count(),
                        StudentsTaughtCount = e.Course.Instructor.OwnedCourses
                            .SelectMany(c => c.Enrollments)
                            .Count(),
                        AverageRating = 4.3f
                    }
                },

                Curriculum = e.Course.Modules.OrderBy(m => m.Order).Select(m => new ModuleSidebarDto
                {
                    ModuleId = m.ModuleId,
                    ModuleTitle = m.Title,
                    ModuleOrder = m.Order,

                    Lessons = m.Lessons.OrderBy(l => l.Order).Select(l => new LessonSidebarDto
                    {
                        LessonId = l.LessonId,
                        LessonTitle = l.Title,
                        LessonOrder = l.Order,

                        TotalDurationInMinutes = (l.LessonContent is VideoContent)
                                        ? (int)Math.Round(((double?)(l.LessonContent as VideoContent).DurationInSeconds ?? 0) / 60.0)
                                        : 0,

                        ContentType = l.ContentType.ToString(),
                    })
                    .ToList()
                })
                .ToList()
            })
            .FirstOrDefaultAsync();

        if (classroomDto == null) 
            return null;

        // 3. Fetch Progress (List of IDs)
        var allLessonIds = classroomDto.Curriculum
            .SelectMany(m => m.Lessons)
            .Select(l => l.LessonId)
            .ToHashSet();

        var completedIds = await progressQuery
            .Where(p => allLessonIds.Contains(p.LessonId) && p.IsCompleted)
            .Select(p => p.LessonId)
            .ToHashSetAsync();

        // 4. The Logic Loop (One Pass to Rule Them All)
        LessonSidebarDto? activeLesson = null;
        bool foundActive = false;

        foreach (var module in classroomDto.Curriculum)
        {
            foreach (var lesson in module.Lessons)
            {
                // A. Mark Complete
                if (completedIds.Contains(lesson.LessonId))
                {
                    lesson.IsCompleted = true;
                    classroomDto.CourseProgress.CompletedLessonsCount++;
                }

                // B. Find Active Lesson
                if (!foundActive)
                {
                    if (lessonId.HasValue)
                    {
                        // User requested specific lesson
                        if (lesson.LessonId == lessonId.Value)
                        {
                            activeLesson = lesson;
                            foundActive = true;
                        }
                    }
                    else
                    {
                        // "Resume" Logic: First Incomplete Lesson
                        if (!lesson.IsCompleted)
                        {
                            activeLesson = lesson;
                            foundActive = true;
                        }
                    }
                }
            }
        }

        // Edge Case: If course is 100% complete, 'activeLesson' might still be null.
        // Default to the very first lesson (Review mode) or very last.
        if (activeLesson == null && classroomDto.Curriculum != null)
            activeLesson = classroomDto.Curriculum
                .LastOrDefault(m => m.Lessons.Count() > 0).Lessons?
                .LastOrDefault();

        if (activeLesson == null)
            return null;


        // 5. Mark Active
        activeLesson.IsActive = true;
        // We also need to mark the parent module active for the accordion to open
        var activeModule = classroomDto.Curriculum.First(m => m.Lessons.Any(l => l.LessonId == activeLesson.LessonId));
        activeModule.IsActive = true;

        // 6. Final Stats Calculation
        classroomDto.CourseOverviewStats.ModulesCount = classroomDto.Curriculum.Count();
        classroomDto.CourseOverviewStats.LessonsCount = classroomDto.Curriculum
            .SelectMany(m => m.Lessons)
            .Count();

        var completedLessonsDurationMinutes = 0;
        var totalDurationMinutes = 0;

        foreach (var module in classroomDto.Curriculum)
        {
            module.TotalDurationInMinutes = 0;

            bool isModuleComplete = true;
            foreach(var lesson in module.Lessons)
            {
                module.TotalDurationInMinutes += lesson.TotalDurationInMinutes;
                totalDurationMinutes += lesson.TotalDurationInMinutes;
                if (lesson.IsCompleted)
                    completedLessonsDurationMinutes += lesson.TotalDurationInMinutes;

                if (isModuleComplete && lesson.IsCompleted == false)
                    isModuleComplete = false;

                module.LessonsCount++;
            }

            module.IsCompleted = isModuleComplete;
        }

        classroomDto.CourseOverviewStats.TotalDurationInMinutes = totalDurationMinutes;

        classroomDto.CourseProgress.TotalLessonsCount = classroomDto.CourseOverviewStats.LessonsCount;
        classroomDto.CourseProgress.ProgressPercentage = (int)Math.Round((double)completedLessonsDurationMinutes / totalDurationMinutes * 100);


        // 7. Fetch the Active Lesson Content (The Player)
        // This is the ONLY extra query needed.
        var CurrentLesson = await _lessonRepo.GetByIdQueryable(activeLesson.LessonId)
            .Select(l => new LessonPlayerDto
            {
                LessonId = activeLesson.LessonId,
                LessonTitle = activeLesson.LessonTitle,
                IsCompleted = activeLesson.IsCompleted,
                ContentType = activeLesson.ContentType,

                ModuleTitle = l.Module.Title,
                ArticleContent = (l.LessonContent is ArticleContent) ? ((ArticleContent)l.LessonContent).Content : null,
                VideoUrl = (l.LessonContent is VideoContent) ? ((VideoContent)l.LessonContent).VideoUrl : null,
                Resources = l.LessonResources.Select(r => new LessonResourceDto
                {
                    LessonResourceId = r.LessonResourceId,
                    ResourceTitle = r.Title,
                    ResourceType = r.ResourceKind.ToString(),
                    ResourceUrl = r.Url
                })
            })
            .FirstOrDefaultAsync();

        classroomDto.CurrentLesson = CurrentLesson!;

        return classroomDto;
    }

    public async Task<LessonPlayerDto?> GetLessonDataAsync(int studentId, int lessonId)
    {
        var containingCourse = await _lessonRepo.GetContainingCourseAsync(lessonId);

        if (containingCourse == null)
            throw new ArgumentException($"No Lesson with Id: {lessonId} was found", nameof(lessonId));

        if (!await _enrollmentRepo.IsStudentEnrolledInCourseAsync(studentId, containingCourse.Id))
            throw new UnauthorizedAccessException($"Student with Id {studentId} is not enrolled in the requested course with Id {containingCourse.Id}");

        // student is enrolled in the course 
        // db calls: 2

        var lessonQuery = _lessonRepo.GetByIdQueryable(lessonId);
        var lessonPlayerInfoDto = await _lessonRepo.GetByIdQueryable(lessonId)
            .Select(l => new LessonPlayerDto
            {
                LessonId = l.LessonId,
                LessonTitle = l.Title,
                ModuleTitle = l.Module.Title,
                ContentType = l.ContentType.ToString(),

                ArticleContent = (l.LessonContent is ArticleContent)
                        ? ((ArticleContent)l.LessonContent).Content
                        : null,
                VideoUrl = (l.LessonContent is VideoContent)
                        ? ((VideoContent)l.LessonContent).VideoUrl
                        : null,

                Resources = l.LessonResources.Select(r => new LessonResourceDto
                {
                    LessonResourceId = r.LessonResourceId,
                    ResourceTitle = r.Title,
                    ResourceType = r.ResourceKind.ToString(),
                    ResourceUrl = r.Url
                }),

                IsCompleted = l.LessonProgresses
                    .Where(p => p.StudentId == studentId)
                    .Select(p => (bool?)p.IsCompleted)
                    .FirstOrDefault() ?? false
            })
            .FirstOrDefaultAsync();

        return lessonPlayerInfoDto;
    }

    public async Task<CourseProgressDto> UpdateLessonCompletionState(int studentId, int lessonId, bool newCompletionState)
    {
        var courseId = await _enrollmentRepo.GetCourseIdIfEnrolledAsync(studentId, lessonId);

        if (courseId == null)
            throw new UnauthorizedAccessException("Student is not enrolled in the course for this lesson.");

        var lessonProgress = await _progressRepo.GetStudentLessonProgressAsync(studentId, lessonId);

        if (lessonProgress == null)
        {
            lessonProgress = new LessonProgress
            {
                StudentId = studentId,
                LessonId = lessonId,

                StartedDate = (newCompletionState) ? DateTime.UtcNow : null,
                CompletedDate = (newCompletionState) ? DateTime.UtcNow : null,
                IsCompleted = newCompletionState,
            };
        } else
        {
            if (newCompletionState != lessonProgress.IsCompleted) {
                lessonProgress.IsCompleted = newCompletionState;
                lessonProgress.CompletedDate = (newCompletionState) ? DateTime.UtcNow : null;
            }
        }

        await _progressRepo.AddOrUpdateAsync(lessonProgress);
        await _unitOfWork.CompleteAsync();


        // completed lessons count
        //      filter lesson progress based on (CourseId == courseId)
        //          then filter lesson progress based on (IsCompleted)
        // total lessons count
        //      filter lesson progress based on (CourseId == courseId)
        // progress percentage    from   done duration / total duration
        //      get total duration for completed lessons count
        //      get total duration for the course
        //          then divide both


        var courseTotals = await _courseRepo.GetCourseByIdQueryable(courseId.Value)
        .Select(c => new
        {
            LessonsCount = c.Modules.SelectMany(m => m.Lessons).Count(),
            TotalDurationSeconds = c.Modules.SelectMany(m => m.Lessons)
                        .Select(l => l.LessonContent)
                        .OfType<VideoContent>()
                        .Sum(v => (int?)v.DurationInSeconds) ?? 0
        })
        .FirstOrDefaultAsync();


        var completedStats = await _progressRepo.GetAllLessonProgressForStudentQueryable(studentId)
        .Where(p => p.Lesson.Module.CourseId == courseId && p.IsCompleted)
        .Select(p => new
        {
            TotalDurationSeconds = (p.Lesson.LessonContent is VideoContent)
                        ? ((VideoContent)p.Lesson.LessonContent).DurationInSeconds
                        : 0
        })
        .ToListAsync();


        // in-memory calculations
        var totalLessonsCount = courseTotals.LessonsCount;
        var completedLessonsCount = completedStats.Count();
        var completedDurationSeconds = completedStats.Sum(p => p.TotalDurationSeconds);

        double progressPercentage = 0;
        if (courseTotals != null && courseTotals.TotalDurationSeconds > 0)
            progressPercentage = (completedDurationSeconds / (double)courseTotals.TotalDurationSeconds) * 100;

        await _enrollmentRepo.UpdateProgressPercentageAsync(studentId, courseId.Value, (decimal)progressPercentage);
        await _unitOfWork.CompleteAsync();

        return new CourseProgressDto
        {
            TotalLessonsCount = totalLessonsCount,
            CompletedLessonsCount = completedLessonsCount,
            ProgressPercentage = (int)Math.Round(progressPercentage, 2)
        };
    }


    public async Task<bool> MarkLessonStartedAsync(int studentId, int lessonId)
    {
        var courseId = await _enrollmentRepo.GetCourseIdIfEnrolledAsync(studentId, lessonId);
        if (courseId == null)
            throw new UnauthorizedAccessException("Student is not enrolled in the course for this lesson, you can't mark the lesson as started");

        var progress = await _progressRepo.GetStudentLessonProgressAsync(studentId, lessonId);
        bool found = (progress != null);

        if (progress == null) {
            progress = new LessonProgress
            {
                StudentId = studentId,
                LessonId = lessonId,

                IsCompleted = false,
                StartedDate = null,
                CompletedDate = null
            };
        }

        // handle the case: start --> complete ---> user marks as uncomplete which makes start date = null & end date = null & is completed = false --> start (start date is null so we set it and we start the lesson for the second time)
        if (progress.StartedDate == null)
            progress.StartedDate = DateTime.UtcNow;
        else
            return true;

        if (!found)
            await _progressRepo.AddAsync(progress);

        return await _unitOfWork.CompleteAsync() > 0;
    }
}
