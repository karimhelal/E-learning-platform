namespace Web.ViewModels.Student
{
    public class StudentTrackDetailsViewModel
    {
        public StudentTrackDetailsData? Data { get; set; }
        public string PageTitle { get; set; } = "Track Details";
    }

    public class StudentTrackDetailsData
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string UserInitials { get; set; } = "JD";

        public int TrackId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public decimal TotalProgress { get; set; } = 0;

        public List<TrackCourseItem> Courses { get; set; } = new();

        // You can expand with more properties if needed
    }

    public class TrackCourseItem
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int DurationHours { get; set; }
        public int LessonsCount { get; set; }
        public decimal ProgressPercentage { get; set; }
    }
}
