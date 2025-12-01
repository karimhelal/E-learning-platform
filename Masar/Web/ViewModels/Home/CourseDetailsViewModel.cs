using BLL.DTOs.Course;

namespace Web.ViewModels.Home;

public class CourseDetailsViewModel
{
    public CourseDetailsDto Course { get; set; }
    public string PageTitle { get; set; }
    public bool IsEnrolled { get; set; } = false;
    public int? StudentEnrollmentId { get; set; }
    public decimal? ProgressPercentage { get; set; }
}