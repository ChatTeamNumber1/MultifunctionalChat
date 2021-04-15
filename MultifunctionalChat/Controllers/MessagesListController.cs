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
        private readonly List<Message> messagesList;

        public MessagesListController(IRepository<Message> messageService, ILogger<MessageController> logger)
        {
            this.messageService = messageService;
            this.logger = logger;
            messagesList = messageService.GetList();
        }

        [HttpGet]
        public ActionResult<List<Message>> Get()
        {
            logger.LogInformation("Выведен полный список сообщений");
            return messagesList;
        }

        [HttpGet("{id}")]
        public ActionResult<List<Message>> Get(int id)
        {
            var msgList = messagesList.Where(mess =>mess.RoomId == id).ToList();
            logger.LogInformation($"Выведены сообщения из комнаты с id = {id}");
            return msgList;
        }
    }
}