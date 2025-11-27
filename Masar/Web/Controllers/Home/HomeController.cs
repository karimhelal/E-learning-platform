using System.Diagnostics;
using Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using DAL.Data;
using Web.Interfaces;


namespace Web.Controllers.Home
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, ICurrentUserService currentUserService)
        {
            _logger = logger;
            _context = context;
            _currentUserService = currentUserService;
        }

        public IActionResult Index()
        {
            var userId = _currentUserService.GetUserId();
            ViewBag.UserId = userId;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}