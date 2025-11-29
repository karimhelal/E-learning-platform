using BLL.Interfaces.Account;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            _authService = authService;
            _signInManager = signInManager;
        }

        [HttpGet]
        [Route("~/Account/Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }

            var signInResult = await _signInManager.PasswordSignInAsync(
                loginViewModel.Email,
                loginViewModel.Password,
                loginViewModel.RememberMe,
                lockoutOnFailure: true);

            if (signInResult.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account is locked. Please try again later.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(loginViewModel);
        }

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
                if(user == null)
                {
                    ModelState.AddModelError(string.Empty, "Registration succeeded but user data is missing.");
                    return View(registerViewModel);
                }

                await _signInManager.SignInAsync(user, registerViewModel.RememberMe);

                return RedirectToAction("Index", "Home");
            }

            //var isEmailTaken = result.Errors.Any(e => e.Code == "DuplicateEmail" || e.Code == "DuplicateUserName");

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
