using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultifunctionalChat.Models;
using MultifunctionalChat.Services;

namespace MultifunctionalChat.Controllers
{
    [Route("[controller]")]
    public class MessagesListController : ControllerBase
    {
        private readonly IRepository<Message> messageService;
        private readonly ILogger<MessageController> logger;

        public MessagesListController(IRepository<Message> messageService, ILogger<MessageController> logger)
        {
            this.messageService = messageService;
            this.logger = logger;
        }

        [HttpGet]
        public ActionResult<List<Message>> Get()
        {
            var messagesList = messageService.GetList();
            logger.LogInformation("Выведен полный список сообщений");
            return messagesList;
        }

        [HttpGet("{id}")]
        public ActionResult<List<Message>> Get(int id)
        {
            var messagesList = messageService.GetList().Where(mess =>mess.RoomId == id).ToList();
            logger.LogInformation($"Выведены сообщения из комнаты с id = {id}");
            return messagesList;
        }
    }
}