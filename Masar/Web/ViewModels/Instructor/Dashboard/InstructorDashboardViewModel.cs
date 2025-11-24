using BLL.DTOs.Instructor;

namespace Web.ViewModels.Instructor.Dashboard;

public class InstructorDashboardViewModel
{
    public InstructorDashboardDataViewModel Data { get; set; }


    // --- UI Specific Properties ---
    public string PageTitle { get; set; }
    public string GreetingMessage { get; set; }
}
