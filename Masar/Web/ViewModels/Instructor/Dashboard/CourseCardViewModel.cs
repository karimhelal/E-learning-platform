using Core.Entities;
using Core.Entities.Enums;

namespace Web.ViewModels.Instructor.Dashboard;

public class CourseCardViewModel
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
