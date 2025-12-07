using Web.ViewModels.Home;
using Web.ViewModels.Misc;
using Web.ViewModels.Misc.FilterRequestVMs;

namespace Web.ViewModels.Student
{
    public class StudentBrowseCoursesViewModel
    {
        public StudentBrowseCoursesPageData Data { get; set; } = new();
        public string PageTitle { get; set; } = "Browse Courses";
    }

    public class StudentBrowseCoursesPageData
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string UserInitials { get; set; } = "JD";

        // Filter and pagination settings (like home browse)
        public BrowseSettingsViewModel Settings { get; set; } = new();
        
        // Course items for display (with enrollment status)
        public IEnumerable<StudentCourseBrowseCardViewModel> Items { get; set; } = new List<StudentCourseBrowseCardViewModel>();
    }

    /// <summary>
    /// Student-specific course card that includes enrollment information
    /// </summary>
    public class StudentCourseBrowseCardViewModel : CourseBrowseCardViewModel
    {
        /// <summary>
        /// Whether the student is enrolled in this course
        /// </summary>
        public bool IsEnrolled { get; set; }
        
        /// <summary>
        /// Progress percentage (0-100) if enrolled
        /// </summary>
        public decimal ProgressPercentage { get; set; }
    }
}