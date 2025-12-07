using BLL.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.ViewModels.Admin;

namespace Web.Controllers.Admin.controller
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public IActionResult Users()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            var categories = await _adminService.GetAllCategoriesAsync();

            var model = new ManageCoursesViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Name 
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Account/AccessDenied")]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Access Denied";
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> PendingCourses()
        {
            var categories = await _adminService.GetAllCategoriesAsync();

            var pendingCount = await _adminService.GetPendingCoursesCountAsync(); 

            var model = new PendingPageViewModel
            {
                Categories = categories.Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Name
                }).ToList(),
                PendingCount = pendingCount
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Tracks()
        {
            return View();
        }

        // Add this action to the MVC AdminController
        [HttpGet]
        public IActionResult CreateTrack()
        {
            return View();
        }
    }
}
