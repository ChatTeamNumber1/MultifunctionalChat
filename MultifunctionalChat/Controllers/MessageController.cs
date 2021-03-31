using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultifunctionalChat.Models;
using MultifunctionalChat.Services;

namespace Warehouse.Controllers
{
    //TODO Потом заменить на то, что сделает Эллина
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IRepository<Message> messagesService;
        private readonly ILogger<MessageController> logger;

        public MessageController(IRepository<Message> messagesService, ILogger<MessageController> logger)
        {
            this.messagesService = messagesService;
            this.logger = logger;
        }

        [HttpGet]
        public ActionResult<List<Message>> Get()
        {
            var dataForReact = messagesService.GetList();
            logger.LogInformation("Пользователи найдены");
            return dataForReact;
        }

        [HttpGet("{id}")]
        public ActionResult<Message> Get(int id)
        {
            Message messages = messagesService.Get(id);
            if (messages == null)
            {
                logger.LogError($"Нет пользователя с id = {id}");
                return NotFound();
            }

            logger.LogInformation($"Получен пользователь с id = {id}");
            return messages;
        }

        [HttpPost]
        public ActionResult<Message> Post(Message messages)
        {
            messagesService.Create(messages);
            logger.LogInformation($"Добавлен пользователь с id = {messages.Id}");
            return Ok($"Добавлен пользователь с id = {messages.Id}");
        }

        [HttpPut("{id}")]
        public ActionResult<Message> Edit(Message messages)
        {
            messagesService.Update(messages);
            logger.LogInformation($"Отредактирован пользователь с id = {messages.Id}");
            return Ok($"Отредактирован пользователь с id = {messages.Id}");
        }

        [HttpDelete("{id}")]
        public ActionResult<Message> Delete(int id)
        {
            Message messages = messagesService.Get(id);
            if (messages == null)
            {
                return NotFound();
            }

            messagesService.Delete(messages.Id);
            return Ok($"Удален пользователь с id = {messages.Id}");
        }
    }
}