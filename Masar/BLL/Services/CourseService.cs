using BLL.DTOs.Course;
using BLL.DTOs.Instructor;
using BLL.DTOs.Misc;
using BLL.Interfaces;
using Core.Entities;
using Core.Entities.Enums;
using Core.RepositoryInterfaces;
using MailKit;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ILanguageRepository _languageRepo;
    public CourseService(ICourseRepository courseRepo, ICategoryRepository categoryRepo, ILanguageRepository languageRepo) {
        _courseRepo = courseRepo;
        _categoryRepo = categoryRepo;
        _languageRepo = languageRepo;
    }

    public async Task<PagedResultDto<InstructorCourseDto>> GetCoursesByInctructorAsync(
        int instructorId, 
        PagingRequestDto request)
    {
        var query = _courseRepo.GetCoursesByInstructorQueryable(instructorId);

        bool isASC = request.SortOrder == SortOrder.Ascending;
        CourseSortOption sortBy = request.SortBy;

        query = sortBy switch
        {
            CourseSortOption.CreationDate => isASC ? query.OrderBy(c => c.CreatedDate)
                                                   : query.OrderByDescending(c => c.CreatedDate),

            CourseSortOption.Popularity => isASC ? query.OrderBy(c => c.Enrollments!.Count())
                                                 : query.OrderByDescending(c => c.Enrollments!.Count()),

            CourseSortOption.Title => isASC ? query.OrderBy(c => c.Title)
                                            : query.OrderByDescending(c => c.Title),

            _ => query.OrderByDescending(c => c.CreatedDate)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((request.CurrentPage - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new InstructorCourseDto
            {
                CourseId = c.Id,
                Title = c.Title,
                Description = c.Description,
                ThumbnailImageUrl = c.ThumbnailImageUrl,
                CreatedDate = c.CreatedDate,
                MainCategory = c.Categories.FirstOrDefault().Name,

                // calculated fields
                NumberOfModules = c.Modules.Count(),
                NumberOfStudents = c.Enrollments!.Count(),
                NumberOfMinutes = (int)(
                                c.Modules
                                    .SelectMany(m => m.Lessons)
                                    .Select(l => l.LessonContent)
                                    .OfType<VideoContent>()
                                    .Sum(l => l.DurationInSeconds) / 60.0)
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResultDto<InstructorCourseDto>
        {
            Items = items,
            Settings = new PaginationSettingsDto
            {
                TotalPages = totalPages,
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                TotalCount = totalCount
            }
        };
    }

    public InstructorCourseBasicDetailsDto? GetInstructorCourseBasicDetails(
        int instructorId,
        int courseId)
    {
        var course = _courseRepo.GetByIdAsync(courseId).Result;

        if (course == null)
            return null;

        return new InstructorCourseBasicDetailsDto
        {
            InstructorId = instructorId,

            CourseId = course.Id,
            Title = course.Title,
            Description = course.Description!,
            Level = course.Level,
            ThumbnailImageUrl = course.ThumbnailImageUrl!,
            MainCategory = course.Categories?.FirstOrDefault()!.Name,
            Categories = course.Categories!.Select(cat => cat.Name),
            LearningOutcomes = course.LearningOutcomes,
        };
    }

    public InstructorCourseContentDetailsDto? GetInstructorCourseContentDetails(
        int instructorId, 
        int courseId)
    {
        var query = _courseRepo.GetCourseByIdQueryable(courseId)
            .Where(c => c.InstructorId == instructorId)
            .Select(c => new InstructorCourseContentDetailsDto
            {
                CourseId = c.Id,
                InstructorId = c.InstructorId,

                ModulesDetails = c.Modules!.Select(m => new ModuleDetailsDto
                {
                    ModuleId = m.ModuleId,
                    CourseId = m.CourseId,
                    ModuleTitle = m.Title,
                    ModuleOrder = m.Order,

                    LessonsDetails = m.Lessons!.Select(l => new LessonDetailsDto 
                    {
                        LessonId = l.LessonId,
                        LessonTitle = l.Title,
                        LessonOrder = l.Order,
                        LessonContentType = l.ContentType,
                        ModuleId = m.ModuleId,

                        DurationInSeconds = l.LessonContent is VideoContent 
                            ? ((VideoContent)l.LessonContent).DurationInSeconds 
                            : 0
                    }),

                    TotalDurationInSeconds = m.Lessons!.Sum(l => 
                        l.LessonContent is VideoContent
                        ? ((VideoContent)l.LessonContent).DurationInSeconds
                        : 0)
                }),
            });

        return query.FirstOrDefault();
    }

    public IQueryable<InstructorTopPerformingCourseDto> GetInstructorTopPerformingCourses(
        int instructorId, 
        int topN)
    {
        var query = _courseRepo.GetCoursesByInstructorQueryable(instructorId);

        var result = query.Take(topN).Select(c => new InstructorTopPerformingCourseDto
        {
            Title = c.Title,
            StudentsEnrolled = c.Enrollments!.Count(),
            AverageRating = (float)Math.Round(Math.Max(3.2, new Random().NextDouble() * 5.0), 2)
        });

        return result;
    }






    public async Task<BrowseResultDto<CourseBrowseCardDto>> GetInitialBrowsePageCoursesAsync(PagingRequestDto request)
    {
        var query = _courseRepo.GetAllQueryable().Where(c=>c.Status==LearningEntityStatus.Published);

        // applying sorting
        bool isASC = request.SortOrder == SortOrder.Ascending;
        CourseSortOption sortBy = request.SortBy;

        query = sortBy switch
        {
            CourseSortOption.CreationDate => isASC ? query.OrderBy(c => c.CreatedDate)
                                                   : query.OrderByDescending(c => c.CreatedDate),

            CourseSortOption.Popularity => isASC ? query.OrderBy(c => c.Enrollments!.Count())
                                                 : query.OrderByDescending(c => c.Enrollments!.Count()),

            CourseSortOption.Title => isASC ? query.OrderBy(c => c.Title)
                                            : query.OrderByDescending(c => c.Title),

            _ => query.OrderByDescending(c => c.CreatedDate)
        };


        // populating the results
        var totalCoursesCount = await query.CountAsync();

        var items = await query
            .Skip((request.CurrentPage - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(course => new CourseBrowseCardDto
            {
                CourseId = course.Id,
                InstructorName = course.Instructor!.User!.FullName,

                Title = course.Title,
                Description = course.Description,
                ThumbnailImageUrl = course.ThumbnailImageUrl,
                CreatedDate = course.CreatedDate,
                MainCategory = course.Categories.Select(cat => cat.Name).FirstOrDefault(),
                Categories = course.Categories.Select(cat => cat.Name),
                Languages = course.Languages.Select(lang => lang.Name),
                Level = course.Level,

                // calculated fields
                AverageRating = 3.7f,// (float)Math.Round(((new Random()).NextDouble() * 5), 1),            // HARD CODED
                NumberOfReviews = 2141,// (new Random()).Next(0, 10001),                          // HARD CODED
                NumberOfStudents = course.Enrollments!.Count(),
                NumberOfMinutes = (int)(
                                course.Modules!
                                    .SelectMany(m => m.Lessons!)
                                    .Select(l => l.LessonContent)
                                    .OfType<VideoContent>()
                                    .Sum(l => l.DurationInSeconds) / 60.0),
                NumberOfLectures = course.Modules!
                    .SelectMany(m => m.Lessons!)
                    .Count()
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCoursesCount / (double)request.PageSize);

        // Pagination data 
        var paginationSettings = new PaginationSettingsDto
        {
            CurrentPage = request.CurrentPage,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            TotalCount = totalCoursesCount
        };


        return new BrowseResultDto<CourseBrowseCardDto>
        {
            Items = items ?? new List<CourseBrowseCardDto>(),
            Settings = new BrowseSettingsDto
            {
                FilterGroups = null,
                PaginationSettings = paginationSettings
            }
        };
    }


    public async Task<BrowseResultDto<CourseBrowseCardDto>> GetAllCoursesFilteredForBrowsingPagedAsync(BrowseRequestDto request)
    {
        var query = _courseRepo.GetAllQueryable().Where(c => c.Status == LearningEntityStatus.Published);


        if (request == null)
        {
            request = new BrowseRequestDto
            {
                FilterGroups = new FilterGroupsDto(),
                PagingRequest = new PagingRequestDto()
            };
        }


        var categories = request
            .FilterGroups?
            .CategoryNames!
            .Select(cat => cat.ToLower())
            .ToList();

        var levels = request
            .FilterGroups?
            .LevelNames!
            .Select(level => level.ToLower())
            .ToList();

        var languages = request
            .FilterGroups?
            .LanguageNames!
            .Select(lang => lang.ToLower())
            .ToList();

        var ratersRange = new
        {
            MinRaters = request.FilterGroups?.MinReviews,
            MaxRaters = request.FilterGroups?.MaxReviews
        };

        var ratingRange = new
        {
            MinRating = request.FilterGroups?.MinRating,
            MaxRating = request.FilterGroups?.MaxRating
        };

        var creationDateRange = new
        {
            MinDate = request.FilterGroups?.MinCreationDate,
            MaxData = request.FilterGroups?.MaxCreationDate
        };

        var durationRange = new
        {
            MinDuration = request.FilterGroups?.MinDuration,
            MaxDuration = request.FilterGroups?.MaxDuration
        };

        var enrollmentsRange = new
        {
            MinEnrollments = request.FilterGroups?.MinEnrollments,
            MaxEnrollments = request.FilterGroups?.MaxEnrollments
        };


        // applying filters
        if (categories != null && categories.Any())
            query = query.Where(c => c.Categories!.Any(cat => categories.Contains(cat.Name.ToLower())));

        if (levels != null && levels.Any())
            query = query.Where(c => levels.Contains(c.Level.ToString().ToLower()));

        if (languages != null && languages.Any())
            query = query.Where(c => c.Languages!.Any(lang => languages.Contains(lang.Name.ToLower())));

        if (creationDateRange != null && creationDateRange.MinDate != null && creationDateRange.MaxData != null)
            query = query.Where(c => c.CreatedDate >= creationDateRange.MinDate && c.CreatedDate <= creationDateRange.MaxData);

        if (durationRange != null && durationRange.MinDuration != null && durationRange.MaxDuration != null)
        {
            var minSeconds = durationRange.MinDuration! * 3600;
            var maxSeconds = durationRange.MaxDuration! * 3600;

            query = from c in query
                    let duration = c.Modules!.SelectMany(m => m.Lessons!)
                                            .Select(l => l.LessonContent)
                                            .OfType<VideoContent>()
                                            .Sum(vc => vc.DurationInSeconds)
                    where duration >= minSeconds && duration <= maxSeconds
                    select c;
        }

        if (enrollmentsRange != null && enrollmentsRange.MinEnrollments != null && enrollmentsRange.MaxEnrollments != null)
            query = query.Where(c => c.Enrollments!.Count() >= enrollmentsRange.MinEnrollments 
                                        && c.Enrollments!.Count() <= enrollmentsRange.MaxEnrollments);

        // applying sorting
        bool isAsc = request?.PagingRequest?.SortOrder == SortOrder.Ascending;
        CourseSortOption sortBy = request?.PagingRequest?.SortBy ?? CourseSortOption.CreationDate;

        query = sortBy switch
        {
            CourseSortOption.CreationDate => isAsc ? query.OrderBy(c => c.CreatedDate)
                                                   : query.OrderByDescending(c => c.CreatedDate),

            CourseSortOption.Popularity => isAsc ? query.OrderBy(c => c.Enrollments!.Count())
                                                 : query.OrderByDescending(c => c.Enrollments!.Count()),

            CourseSortOption.Title => isAsc ? query.OrderBy(c => c.Title)
                                            : query.OrderByDescending(c => c.Title),

            _ => query.OrderByDescending(c => c.CreatedDate)
        };


        // populating the results
        var totalCoursesCount = await query.CountAsync();
        var skip = ((request?.PagingRequest.CurrentPage - 1) * request?.PagingRequest.PageSize) ?? 0;
        var take = (request?.PagingRequest.PageSize) ?? totalCoursesCount;

        var items = await query
            .Skip(skip)
            .Take(take)
            .Select(course => new CourseBrowseCardDto
            {
                CourseId = course.Id,
                InstructorName = course.Instructor!.User!.FullName,

                Title = course.Title,
                Description = course.Description,
                ThumbnailImageUrl = course.ThumbnailImageUrl,
                CreatedDate = course.CreatedDate,
                MainCategory = course.Categories.Select(cat => cat.Name).FirstOrDefault(),
                Categories = course.Categories.Select(cat => cat.Name),
                Languages = course.Languages.Select(lang => lang.Name),
                Level = course.Level,

                // calculated fields
                AverageRating = 4.7f, // (float)Math.Round(((new Random()).NextDouble() * 5), 1),            // HARD CODED
                NumberOfReviews = 2156, //(new Random()).Next(0, 10001),                          // HARD CODED
                NumberOfStudents = course.Enrollments!.Count(),
                NumberOfMinutes = (int)(
                                course.Modules!
                                    .SelectMany(m => m.Lessons!)
                                    .Select(l => l.LessonContent)
                                    .OfType<VideoContent>()
                                    .Sum(l => l.DurationInSeconds) / 60.0),
                NumberOfLectures = course.Modules!
                    .SelectMany(m => m.Lessons!)
                    .Count()
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCoursesCount / (double)request.PagingRequest.PageSize);

        // Pagination data 
        var paginationSettings = new PaginationSettingsDto
        {
            CurrentPage = request.PagingRequest.CurrentPage,
            PageSize = request.PagingRequest.PageSize,
            TotalPages = totalPages,
            TotalCount = totalCoursesCount
        };


        return new BrowseResultDto<CourseBrowseCardDto>
        {
            Items = items,
            Settings = new BrowseSettingsDto
            {
                FilterGroups = null,
                PaginationSettings = paginationSettings
            }
        };
    }

    public async Task<FilterGroupsDto> GetFilterSectionConfig()
    {
        var coursesQuery = _courseRepo.GetAllQueryable();
        var categriesQuery = _categoryRepo.GetAllQueryable();
        var languagesQuery = _languageRepo.GetAllQueryable();

        var result = new FilterGroupsDto();

        result.CategoryNames = await categriesQuery.Select(c => c.Name).ToListAsync();
        result.LanguageNames = await languagesQuery.Select(l => l.Name).ToListAsync();
        result.LevelNames = new List<string> { "Beginner", "Intermediate", "Advanced" };


        var durationsQuery = coursesQuery
            .Select(c => c.Modules!
                .SelectMany(m => m.Lessons!)
                .Select(l => l.LessonContent)
                .OfType<VideoContent>()
                .Sum(v => (int?)v.DurationInSeconds) ?? 0
            );
        result.MinDuration = Math.Floor((durationsQuery.Select(x => (int?)x).Min() ?? 0) / 3600.0);
        result.MaxDuration = Math.Ceiling((durationsQuery.Select(x => (int?)x).Max() ?? 0) / 3600.0);


        var enrollmentsQuery = coursesQuery.Select(c => c.Enrollments!.Count());
        result.MinEnrollments = enrollmentsQuery.Min();
        result.MaxEnrollments = enrollmentsQuery.Max();


        result.MinCreationDate = coursesQuery.Min(c => c.CreatedDate);
        result.MaxCreationDate = coursesQuery.Max(c => c.CreatedDate);

        result.MinRating = 0f;
        result.MaxRating = 5f;

        result.MinReviews = 0;
        result.MaxReviews = 1000;

        result.HasCertificate = null;

        return result;
    }

    public async Task<FilterGroupsStatsDto> GetFilterGroupsStats()
    {
        var query = _courseRepo.GetAllQueryable();
        var result = new FilterGroupsStatsDto();

        result.CategoryCounts = await query
            .SelectMany(c => c.Categories!)
            .GroupBy(c => c.Name)
            .Select(g => new
            {
                Id = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.Id, x => x.Count);


        result.LevelCounts = await query
            .Select(c => c.Level.ToString().ToLower())
            .GroupBy(l => l)
            .Select(g => new
            {
                Id = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.Id, x => x.Count);

        result.LanguageCounts = await query
            .SelectMany(c => c.Languages!)
            .GroupBy(l => l.Name)
            .Select(g => new
            {
                Id = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.Id, x => x.Count);

        return result;
    }



    //public async Task<FilterGroupsDto> GetFilterSectionConfig()
    //{
    //    var result = new FilterGroupsDto();

    //    var CategoryNames = await _categoryRepo.GetAllQueryable().Select(c => c.Name).ToListAsync();
    //    var Languages = await _languageRepo.GetAllQueryable().Select(l => l.Name).ToListAsync();
    //    var Levels = new List<string> { "Beginner", "Intermediate", "Advanced" };

    //    var MinDuration = Math.Ceiling(_courseRepo.GetAllQueryable()
    //        .Min(c => c.Modules!
    //            .SelectMany(m => m.Lessons!)
    //            .Select(l => l.LessonContent)
    //            .OfType<VideoContent>()
    //            .Sum(v => v.DurationInSeconds)
    //        ) / (double)3600);

    //    var MaxDuration = Math.Ceiling(_courseRepo.GetAllQueryable()
    //        .Min(c => c.Modules!
    //            .SelectMany(m => m.Lessons!)
    //            .Select(l => l.LessonContent)
    //            .OfType<VideoContent>()
    //            .Sum(v => v.DurationInSeconds)
    //        ) / (double)3600);

    //    var MinEnrollments = _courseRepo.GetAllQueryable().Min(c => c.Enrollments!.Count());
    //    var MaxEnrollments = _courseRepo.GetAllQueryable().Max(c => c.Enrollments!.Count());

    //    var MinDate = _courseRepo.GetAllQueryable().Min(c => c.CreatedDate);
    //    var MaxDate = _courseRepo.GetAllQueryable().Max(c => c.CreatedDate);

    //    var MinRating = 0;
    //    var MaxRating = 5;

    //    result.FilterGroups.Add(new CheckBoxFilter
    //    {
    //        Title = "Categories",
    //        RequestKey = "CategoryNames",
    //        UiType = "Checkbox",

    //        FilterOptions = CategoryNames.Select(cat => new FilterOption
    //        {
    //            Label = cat,
    //            Count = 0,
    //            IsChecked = false,
    //            Value = ""
    //        })
    //    });

    //    result.FilterGroups.Add(new CheckBoxFilter
    //    {
    //        Title = "Levels",
    //        RequestKey = "LevelNames",
    //        UiType = "Checkbox",

    //        FilterOptions = Levels.Select(lev => new FilterOption
    //        {
    //            Label = lev,
    //            Count = 0,
    //            IsChecked = false,
    //            Value = ""
    //        })
    //    });

    //    result.FilterGroups.Add(new CheckBoxFilter
    //    {
    //        Title = "Languages",
    //        RequestKey = "LanguageNames",
    //        UiType = "Checkbox",

    //        FilterOptions = Languages.Select(lang => new FilterOption
    //        {
    //            Label = lang,
    //            Count = 0,
    //            IsChecked = false,
    //            Value = ""
    //        })
    //    });


    //    result.FilterGroups.Add(new RangeFilter<double>
    //    {
    //        Title = "Duration (Hours)",
    //        RequestKey = "Duration",
    //        UiType = "Range",

    //        MinRequestKey = "MinDuration",
    //        MaxRequestKey = "MaxDuration",
    //        MinValue = MinDuration,
    //        MaxValue = MaxDuration,
    //        Unit = "H"
    //    });


    //    result.FilterGroups.Add(new RangeFilter<int>
    //    {
    //        Title = "Enrollments",
    //        RequestKey = "Enrollments",
    //        UiType = "Range",

    //        MinRequestKey = "MinEnrollments",
    //        MaxRequestKey = "MaxEnrollments",
    //        MinValue = MinEnrollments,
    //        MaxValue = MaxEnrollments,
    //        Unit = "STUDENTS"
    //    });


    //    result.FilterGroups.Add(new RangeFilter<float>
    //    {
    //        Title = "Rating",
    //        RequestKey = "Rating",
    //        UiType = "Range",

    //        MinRequestKey = "MinRating",
    //        MaxRequestKey = "MaxRating",
    //        MinValue = MinRating,
    //        MaxValue = MaxRating,
    //        Unit = "⭐"
    //    });


    //    result.FilterGroups.Add(new RangeFilter<DateOnly>
    //    {
    //        Title = "Creation Date",
    //        RequestKey = "CreationDate",
    //        UiType = "Range",

    //        MinRequestKey = "MinCreationDate",
    //        MaxRequestKey = "MaxCreationDate",
    //        MinValue = MinDate,
    //        MaxValue = MaxDate,
    //        Unit = ""
    //    });

    //    return result;
    //}



    private static CourseBrowseCardDto MapToCourseBrowseCard (Course course)
    {
        return new CourseBrowseCardDto
        {
            CourseId = course.Id,
            InstructorName = course.Instructor!.User!.FullName,

            Title = course.Title,
            Description = course.Description,
            ThumbnailImageUrl = course.ThumbnailImageUrl,
            CreatedDate = course.CreatedDate,
            MainCategory = course.Categories.FirstOrDefault().Name,
            Categories = course.Categories.Select(cat => cat.Name),
            Languages = course.Languages.Select(lang => lang.Name),
            Level = course.Level,

            // calculated fields
            AverageRating = (float)Math.Round(((new Random()).NextDouble() * 5), 1),            // HARD CODED
            NumberOfReviews = (new Random()).Next(0, 10001),                          // HARD CODED
            NumberOfStudents = course.Enrollments!.Count(),
            NumberOfMinutes = (int)(
                                course.Modules!
                                    .SelectMany(m => m.Lessons!)
                                    .Select(l => l.LessonContent)
                                    .OfType<VideoContent>()
                                    .Sum(l => l.DurationInSeconds) / 60.0),
            NumberOfLectures = course.Modules!
                    .SelectMany(m => m.Lessons!)
                    .Count()
        };
    }

    public async Task<CourseDetailsDto?> GetCourseDetailsByIdAsync(int courseId)
    {
        var course = await _courseRepo
            .GetCourseByIdQueryable(courseId)
            .Include(c => c.Instructor!)
                .ThenInclude(i => i.User)
            .Include(c => c.Categories)
            .Include(c => c.Languages)
            .Include(c => c.LearningOutcomes)
            .Include(c => c.Modules!)
                .ThenInclude(m => m.Lessons!)
                    .ThenInclude(l => l.LessonContent)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync();

        if (course == null)
            return null;

        var totalDurationMinutes = course.Modules!
            .SelectMany(m => m.Lessons!)
            .Select(l => l.LessonContent)
            .OfType<VideoContent>()
            .Sum(vc => vc.DurationInSeconds) / 60;

        return new CourseDetailsDto
        {
            CourseId = course.Id,
            Title = course.Title,
            Description = course.Description,
            ThumbnailImageUrl = course.ThumbnailImageUrl,
            Level = course.Level,
            CreatedDate = course.CreatedDate,
            
            // Instructor
            InstructorName = course.Instructor!.User!.FullName,
            InstructorBio = course.Instructor.Bio ?? "No bio available",
            InstructorYearsOfExperience = course.Instructor.YearsOfExperience ?? 0,
            
            // Categories & Languages
            MainCategory = course.Categories!.FirstOrDefault()!.Name,
            Categories = course.Categories!.Select(cat => cat.Name),
            Languages = course.Languages!.Select(lang => lang.Name),
            
            // Learning Outcomes
            LearningOutcomes = course.LearningOutcomes,
            
            // Modules
            Modules = course.Modules!
                .OrderBy(m => m.Order)
                .Select(m => new ModuleWithLessonsDto
                {
                    ModuleId = m.ModuleId,
                    Title = m.Title,
                    Description = m.Description,
                    Order = m.Order,
                    DurationMinutes = m.Lessons!
                        .Select(l => l.LessonContent)
                        .OfType<VideoContent>()
                        .Sum(vc => vc.DurationInSeconds) / 60,
                    Lessons = m.Lessons!
                        .OrderBy(l => l.Order)
                        .Select(l => new LessonSummaryDto
                        {
                            LessonId = l.LessonId,
                            Title = l.Title,
                            Order = l.Order,
                            ContentType = l.ContentType,
                            DurationSeconds = l.LessonContent is VideoContent 
                                ? ((VideoContent)l.LessonContent).DurationInSeconds 
                                : 0
                        })
                }),
            
            // Statistics
            TotalStudents = course.Enrollments!.Count(),
            TotalModules = course.Modules!.Count(),
            TotalLessons = course.Modules!.SelectMany(m => m.Lessons!).Count(),
            TotalDurationMinutes = totalDurationMinutes,
            AverageRating = (float)Math.Round(new Random().NextDouble() * 5, 1), // TODO: Replace with actual rating
            NumberOfReviews = new Random().Next(0, 5000) // TODO: Replace with actual reviews
        };
    }
}
