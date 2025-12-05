using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.ViewModels.Admin
{
    public class PendingPageViewModel
    {
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public int PendingCount { get; set; }
    }
}
