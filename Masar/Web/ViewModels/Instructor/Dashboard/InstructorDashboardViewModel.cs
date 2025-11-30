using BLL.DTOs.Instructor;

namespace Web.ViewModels.Instructor.Dashboard;

public class InstructorDashboardViewModel
{
    public InstructorDashboardDataViewModel Data { get; set; }


    // --- UI Specific Properties ---
    public string PageTitle { get; set; }
    public string GreetingMessage { get; set; }
}

public class InstructorDashboardDataViewModel
{
    public int InstructorId { get; set; }
    public string InstructorName { get; set; }

    // Stats
    public InstructorGeneralStatsViewModel GeneralStats { get; set; }
    public InstructorCurrentMonthStatsViewModel CurrentMonthStats { get; set; }
    public IEnumerable<InstructorTopPerformingCourseViewModel> TopPerformingCourses { get; set; }
    public IEnumerable<InstructorCourseCardViewModel> CourseCards { get; set; }
}


public class InstructorGeneralStatsViewModel
{
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public float AverageRating { get; set; }
    public int CompletionRate { get; set; }
}


public class InstructorCurrentMonthStatsViewModel
{
    public int NewStudents { get; set; }
    public int Completions { get; set; }
    public int NewReviews { get; set; }
}


public class InstructorTopPerformingCourseViewModel
{
    public string Title { get; set; }
    public int StudentsEnrolled { get; set; }
    public float AverageRating { get; set; }
}


public class InstructorCourseCardViewModel
{
    public int CourseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string MainCategory { get; set; }
    public string Level { get; set; }
    public string Status { get; set; }
    public float Rating { get; set; }

    // Stats
    public int StudentsCount { get; set; }
    public int ModulesCount { get; set; }
    public double DurationHours { get; set; }
}
