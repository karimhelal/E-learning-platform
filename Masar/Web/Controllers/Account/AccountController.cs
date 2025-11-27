using BLL.Interfaces.Account;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;
using Web.ViewModels.Account;
using BLL.DTOs.Account;

namespace Web.Controllers.Account
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<User> _signInManager;

        public AccountController(IAuthService authService, SignInManager<User> signInManager)
        {
            this._authService = authService;
            this._signInManager = signInManager;
        }
        [HttpGet]
        [Route("/Account/Login")]
        public IActionResult Login()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Login(LoginViewModel loginViewModel)
        //{
        //    return View();
        //}

        [HttpGet]
        [Route("/Account/Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(registerViewModel);
            }

            //check if this email register before
            ///
            ///
            //



            var RegisterDto = new RegisterDto
            {
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                Email = registerViewModel.Email,
                Password = registerViewModel.Password
            };

            var (result,user) = await _authService.RegisterUserAsync(RegisterDto);
            if(result.Succeeded)
            {
                await _signInManager.SignInAsync(user,registerViewModel.RememberMe);
                return RedirectToAction("Index", "Home");
            }

            var isEmailTaken = result.Errors.Any(e => e.Code == "DuplicateEmail" || e.Code == "DuplicateUserName");

            if (result.Errors.Any(e => e.Code == "DuplicateEmail" || e.Code == "DuplicateUserName"))
            {
                // نضع الرسالة في ViewBag عشان نستقبلها في الـ View
                ViewBag.AlertMessage = "This email address is already registered, please log in.";
                ViewBag.AlertType = "warning"; // نوع التنبيه
            }
            else
            {
                // لو فيه أخطاء تانية (زي الباسورد) نعرضها بالطريقة العادية
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(registerViewModel);
        }

        [HttpGet]
        [Route("/Account/Forgot-Password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }
    }
}
