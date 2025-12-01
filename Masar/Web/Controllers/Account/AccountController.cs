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
        private readonly IEmailService _emailService;

        public AccountController(IAuthService authService, SignInManager<User> signInManager, IEmailService emailService)
        {
            _authService = authService;
            _signInManager = signInManager;
            _emailService = emailService;
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
        [Route("/Account/ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var dto = new ForgotPasswordDto
            {
                Email = model.Email
            };
            var token = await _authService.GeneratePasswordResetTokenAsync(dto.Email);

            if (token != null)
            {
                var link = Url.Action("ResetPassword", "Account", new { email = model.Email, token = token }, Request.Scheme);
                await _emailService.SendEmailAsync(model.Email, "Reset Password", $"<a href='{link}'>Reset Password</a>");
            }
            return View("ForgotPasswordConfirmation");
        }


        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (email == null || token == null) return BadRequest();

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);


            var dto = new ResetPasswordDto
            {
                Email = model.Email,
                Token = model.Token,
                NewPassword = model.Password
            };

            var result = await _authService.ResetPasswordAsync(dto);

            if (result.Succeeded)
            {
                return View("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }





        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [Route("/Account/Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return View("Login");
        }
    }
}
