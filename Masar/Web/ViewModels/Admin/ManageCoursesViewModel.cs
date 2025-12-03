using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Admin
{
    public class ManageCoursesViewModel
    {
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }
}
