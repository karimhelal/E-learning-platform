namespace BLL.DTOs.Instructor;

public class InstructorDashboardDto
{
    public int InstructorId { get; set; }
    public string InstructorName { get; set; }

    // Stats
    public InstructorGeneralStatsDto GeneralStats { get; set; }
    public InstructorCurrentMonthStatsDto CurrentMonthStats { get; set; }
    public IEnumerable<InstructorTopPerformingCourseDto> TopPerformingCourses { get; set; }
    public IEnumerable<InstructorCourseCardDto> CourseCards { get; set; }
}


public class InstructorGeneralStatsDto
{
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public float AverageRating { get; set; }
    public int CompletionRate { get; set; }
}

public class InstructorCurrentMonthStatsDto
{
    public int NewStudents { get; set; }
    public int Completions { get; set; }
    public int NewReviews { get; set; }
}


public class InstructorTopPerformingCourseDto
{
    public string Title { get; set; }
    public int StudentsEnrolled { get; set; }
    public float AverageRating { get; set; }
}


public class InstructorCourseCardDto
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
