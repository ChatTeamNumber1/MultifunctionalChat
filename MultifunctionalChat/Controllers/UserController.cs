using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultifunctionalChat.Models;
using MultifunctionalChat.Services;

namespace MultifunctionalChat.Controllers
{
    public class UserController : Controller
    {
        private readonly IRepository<User> usersService;
        private readonly ILogger<UserController> logger;

        public UserController(IRepository<User> usersService, ILogger<UserController> logger)
        {
            this.usersService = usersService;
            this.logger = logger;
        }

        public ActionResult AllUsers(string id)
        {
            ViewBag.users = usersService.GetList();
            return PartialView("AllUsers");
        }

        [HttpGet]
        public ActionResult<List<User>> Get()
        {
            var dataForReact = usersService.GetList();
            logger.LogInformation("Пользователи найдены");
            return dataForReact;
        }

        [HttpGet("{id}")]
        public ActionResult<User> Get(int id)
        {
            User users = usersService.Get(id);
            if (users == null)
            {
                logger.LogError($"Нет пользователя с id = {id}");
                return NotFound();
            }

            logger.LogInformation($"Получен пользователь с id = {id}");
            return users;
        }

        [HttpPost]
        public ActionResult<User> Post(User users)
        {
            usersService.Create(users);
            logger.LogInformation($"Добавлен пользователь с id = {users.Id}");
            return Ok($"Добавлен пользователь с id = {users.Id}");
        }

        [HttpPut("{id}")]
        public ActionResult<User> Edit(User users)
        {
            usersService.Update(users);
            logger.LogInformation($"Отредактирован пользователь с id = {users.Id}");
            return Ok($"Отредактирован пользователь с id = {users.Id}");
        }

        [HttpDelete("{id}")]
        public ActionResult<User> Delete(int id)
        {
            User users = usersService.Get(id);
            if (users == null)
            {
                logger.LogError($"Нет пользователя с id = {id}");
                return NotFound();
            }

            usersService.Delete(users.Id);
            logger.LogInformation($"Удален пользователь с id = {users.Id}");
            return Ok($"Удален пользователь с id = {users.Id}");
        }
    }
}