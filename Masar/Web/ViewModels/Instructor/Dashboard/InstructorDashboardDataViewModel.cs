using BLL.DTOs.Instructor;

namespace Web.ViewModels.Instructor.Dashboard;

public class InstructorDashboardDataViewModel
{
    public int InstructorId { get; set; }
    public string InstructorName { get; set; }

    // Stats
    public InstructorGeneralStatsViewModel GeneralStats { get; set; }
    public InstructorCurrentMonthStatsViewModel CurrentMonthStats { get; set; }
    public IEnumerable<InstructorTopPerformingCourseViewModel> TopPerformingCourses { get; set; }
    public IEnumerable<CourseCardViewModel> CourseCards { get; set; }
}
