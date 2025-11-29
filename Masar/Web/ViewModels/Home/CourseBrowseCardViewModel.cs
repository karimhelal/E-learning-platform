namespace Web.ViewModels.Home;

public class CourseBrowseCardViewModel
{
    public int CourseId { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public string ThumbnailImageUrl { get; set; }
    public string InstructorName { get; set; }
    public string CreatedDate { get; set; }
    public string MainCategory { get; set; }
    public IEnumerable<string> Categories { get; set; }
    public IEnumerable<string> Languages { get; set; }
    public string Level { get; set; }


    // calculated fields
    public float AverageRating { get; set; }
    public int NumberOfReviews { get; set; }

    public int NumberOfLectures { get; set; }
    public int NumberOfStudents { get; set; }
    public int NumberOfMinutes { get; set; }
    public int NumberOfHours => NumberOfMinutes / 60;
}
