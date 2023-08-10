using Blog.Interfaces;
using Blog.Models;
using Blog.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blog.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMembership _membership;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IMembership membership)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _membership = membership;
        }

        [Route("login")]
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [Route("login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    // проверяем, принадлежит ли URL приложению
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный логин и (или) пароль");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }

        [Route("register")]
        [HttpGet]
        public async Task<IActionResult> Register(string? code)
        {
            if (User.Identity.IsAuthenticated || code == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (await _membership.ExistsMembershipByCodeAsync(code))
            {
                if (await _membership.EnableCodeMembershipByCodeAsync(code))
                {
                    return View(new RegisterUserViewModel { Code = code });
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Route("register")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Register(RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userCheck = await _userManager.FindByEmailAsync(model.Email);
                if (userCheck != null)
                {
                    ModelState.AddModelError("Email", "Такой email адрес уже есть!");
                    return View(model);
                }
                User user = new User { Email = model.Email, UserName = model.Email, Name = model.Name, PhoneNumber = model.Phone };
                // добавляем пользователя
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //Установки роли. Сама роль находится в таблице AspNetRoles
                    //если таблица пустая, получим ошибку. Обязательно заполняем роли!
                    result = await _userManager.AddToRoleAsync(user, "Editor");
                    //используем приглашение
                    await _membership.DisableMembershipCodeAsync(model.Code);
                    //установка куки
                    await _signInManager.SignInAsync(user, true);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [Route("settings")]
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }
            var currentUser = await _userManager.FindByIdAsync(userId);
            return View(currentUser);
        }

        [Route("change-password")]
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string id)
        {
            User user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ChangePasswordViewModel model = new ChangePasswordViewModel { Id = user.Id, Email = user.Email };
            return View(model);
        }

        [Route("change-password")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    IdentityResult result =
                        await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Settings");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            if(error.Description.Equals("Incorrect password."))
                            {
                                ModelState.AddModelError(string.Empty, "Введен не верный текущий пароль.");
                                continue;
                            }
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Пользователь не найден");
                }
            }
            return View(model);
        }
    }
}
