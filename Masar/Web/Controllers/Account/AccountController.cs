using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Account
{
    public class AccountController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //[HttpPost]
        //public IActionResult Login(LoginViewModel loginViewModel)
        //{
        //    return View();
        //}

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        //[HttpPost]
        //public IActionResult Register(RegisterViewModel registerViewModel)
        //{
        //    return View();
        //}
    }
}
