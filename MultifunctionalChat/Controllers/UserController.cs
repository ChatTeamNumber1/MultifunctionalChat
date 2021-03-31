using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultifunctionalChat.Models;
using MultifunctionalChat.Services;

namespace Warehouse.Controllers
{
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        //ToDo IRepository<User>
        private readonly UserService usersService;
        private readonly ILogger<UserController> logger;

        public UserController(UserService usersService, ILogger<UserController> logger)
        {
            this.usersService = usersService;
            this.logger = logger;
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