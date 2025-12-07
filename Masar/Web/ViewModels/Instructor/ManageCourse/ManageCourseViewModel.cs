using Core.Entities.Enums;

namespace Web.ViewModels.Instructor.ManageCourse;

public class ManageCourseViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; }
    public string CourseStatus { get; set; }

    public ManageCourseCurriculumViewModel? Curriculum { get; set; }
}


public class ManageCourseCurriculumViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; }
    public string CourseStatus { get; set; }


    public ManageCourseCurriculumStatsViewModel CurriculumStats { get; set; }
    public IEnumerable<ManageViewModuleViewModel> Modules { get; set; }
}


public class ManageCourseCurriculumStatsViewModel
{
    public int TotalModules { get; set; }
    public int VideoLessonsCount { get; set; }
    public int ArticleLessonsCount { get; set; }

    public string FormattedCourseDuration { get; set; }
}


public class ManageViewModuleViewModel
{
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; }

    public int ModuleOrder { get; set; }
    public int LessonsCount { get; set; }
    public string FormattedLessonDuration { get; set; }

    public IEnumerable<ManageViewLessonViewModel> Lessons { get; set; }
}

public class ManageViewLessonViewModel
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; }

    public int LessonOrder { get; set; }
    public string ContentType { get; set; }
    public string FormattedLessonDuration { get; set; }
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////



public class ManageCourseBasicInfoViewModel
{
    public int CourseId { get; set; }
    public string CourseStatus { get; set; }
    
    public string CourseTitle { get; set; }
    public string CourseDescription { get; set; }

    public CourseLevel Level { get; set; }
    public List<ManageCourseCategoryViewModel> Categories { get; set; }
    public List<ManageCourseLanguageViewModel> Languages { get; set; }

    public List<ManageCourseCategoryViewModel> AllCategories { get; set; }
    public List<ManageCourseLanguageViewModel> AllLanguages { get; set; }
    public List<CourseLevel> AllLevels { get; set; }
}


public class ManageCourseCategoryViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
}

public class ManageCourseLanguageViewModel
{
    public int LanguageId { get; set; }
    public string LanguageName { get; set; }
}



//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////



public class ManageCourseLearningOutcomesViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; }
    public string CourseStatus { get; set; }

    public List<CourseLearningOutcomeViewModel> LearningOutcomes { get; set; }
}

public class CourseLearningOutcomeViewModel
{
    public int LearningOutcomeId { get; set; }

    public string OutcomeName { get; set; }
}