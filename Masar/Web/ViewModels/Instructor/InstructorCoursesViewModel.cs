using BLL.DTOs.Course;
using BLL.DTOs.Misc;

namespace Web.ViewModels.Instructor;

public class InstructorCoursesViewModel
{
    public PagedResultViewModel<InstructorCourseViewModel> Data { get; set; }

    // UI specific properties
    public string PageTitle { get; set; }
}
