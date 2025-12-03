namespace Web.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public List<HomeFeaturedTrackViewModel> FeaturedTracks { get; set; } = new();
    }

    public class HomeFeaturedTrackViewModel
    {
        public int TrackId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string IconClass { get; set; } = "fa-laptop-code";
        public int CoursesCount { get; set; }
        public int DurationHours { get; set; }
        public int StudentsCount { get; set; }
    }
}