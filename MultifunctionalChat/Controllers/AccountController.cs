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
        private readonly IRepository<Room> _roomService;

        public AccountController(IRepository<User> context, IRepository<Room> roomService)
        {
            service = context;
            _roomService = roomService;
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
            var chatRooms = _roomService.GetList();
            ViewBag.chatRooms = chatRooms;

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
                    return RedirectToAction("Index", "Account");// переадресация на метод Index
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
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }


    }
}
