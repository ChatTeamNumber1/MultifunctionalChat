using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultifunctionalChat.Services;
using MultifunctionalChat.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace MultifunctionalChat.Controllers
{
    public class AccountController : Controller
    {
        private IRepository<User> service;

        public AccountController(IRepository<User> context)
        {
            service = context;
        }


        public static string EncryptPassword(string Password)
        {
            var data = Encoding.UTF8.GetBytes(Password);
            using (SHA512 shaM = new SHA512Managed())
            {
                var hashedInputBytes = shaM.ComputeHash(data);
                var hashedInputStringBuilder = new StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        // тестирование SignalR

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = service.GetList().FirstOrDefault(x => x.Login == model.Login && x.Password == EncryptPassword(model.Password));
                if (user != null)
                {
                    await Authenticate(user); // аутентификация
                    return RedirectToAction("Index", "Room", new { id = StaticVars.DEFAULT_ROOM_ID });// переадресация на метод Index
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new User { Name = model.Name, Login = model.Login, Password = EncryptPassword(model.Password), RoleId = Convert.ToInt32(StaticVars.ROLE_USER) };
                // добавляем пользователя
                if (service.GetList().FirstOrDefault(x => x.Login == model.Login) == null)
                {
                    service.Create(user);
                    await Authenticate(user); // аутентификация
                    return RedirectToAction("Index", "Room", StaticVars.DEFAULT_ROOM_ID);// переадресация на метод Index
                }
                ModelState.AddModelError("", "Пользователь с таким логином уже существует");
            }
            return View(model);
        }
    }
}
