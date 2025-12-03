namespace Web.ViewModels.Student
{
    public class StudentBrowseCoursesViewModel
    {
        public StudentBrowseCoursesPageData Data { get; set; } = new();
    }

    public class StudentBrowseCoursesPageData
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string UserInitials { get; set; } = "JD";

        public List<BrowseCourseItem> Courses { get; set; } = new();
        public BrowseCoursePageStats Stats { get; set; } = new();
    }

    public class BrowseCoursePageStats
    {
        public int TotalCourses { get; set; }
        public int BeginnerCourses { get; set; }
        public int IntermediateCourses { get; set; }
        public int AdvancedCourses { get; set; }
    }

    public class BrowseCourseItem
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Level { get; set; } = "Beginner";
        public string CategoryName { get; set; } = "General";
        public string CategoryIcon { get; set; } = "fa-book";
        public string CategoryBadgeClass { get; set; } = "badge-purple";
        public string LevelBadgeClass { get; set; } = "beginner";
        public string InstructorName { get; set; } = string.Empty;
        public string? ThumbnailImageUrl { get; set; }
        public int TotalLessons { get; set; }
        public int DurationHours { get; set; }
        public int StudentsCount { get; set; }
        public decimal Rating { get; set; } = 4.8m;
        public string ActionText { get; set; } = "Enroll Now";
        public string ActionUrl { get; set; } = "#";
    }
}