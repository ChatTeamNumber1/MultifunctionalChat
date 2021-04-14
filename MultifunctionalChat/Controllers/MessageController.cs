using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultifunctionalChat.Models;
using MultifunctionalChat.Services;

namespace MultifunctionalChat.Controllers
{
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IRepository<Message> messageService;
        private readonly IRepository<Room> roomService;
        private readonly IRepository<RoomUser> roomUserService;
        private readonly IRepository<User> userService;
        private readonly ILogger<MessageController> logger;

        public MessageController(IRepository<Message> messageService, IRepository<Room> roomService,
            IRepository<User> userService, IRepository<RoomUser> roomUserService, ILogger<MessageController> logger)
        {
            this.messageService = messageService;
            this.roomService = roomService;
            this.roomUserService = roomUserService;
            this.userService = userService;
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
        public ActionResult<Message> Get(int id)
        {
            var message = messageService.Get(id);
            logger.LogInformation($"Выведено сообщение с id = {id}");
            return message;
        }

        [HttpPost]
        public ActionResult<Message> Post(Message message)
        {
            string result = "";

            //Обычные сообщения
            if (!message.Text.StartsWith("//"))
            {
                messageService.Create(message);
                result = $"Сообщение с id = {message.Id} добавлено в общий список";
            }
            //Команды
            else
            {
                string[] messageParts = message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (messageParts[0] == "//room")
                {
                    if (messageParts.Length > 1 && messageParts[1] == "rename")
                    {
                        //Тут через || идут названия двух комнат.
                        string trimmedMessage = message.Text.Replace("//room", "").Replace("rename", "").Trim();
                        string[] renamedParts = trimmedMessage.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

                        //TODO Проверку ошибок бы всю в одно место, а то разбросало...
                        if (renamedParts.Length < 2)
                        {
                            result = "Неверный формат сообщения (не найдены ||)";
                            logger.LogInformation(result);
                            return Ok(result);
                        }

                        var roomToRename = roomService.GetList().Where(
                            room => room.Name == renamedParts[0].Trim()).FirstOrDefault();
                        var userTryingToRename = userService.GetList().Where(
                            user => user.Name == renamedParts[1].Trim()).FirstOrDefault();

                        if (roomToRename == null)
                        {
                            result = $"Неверное название комнаты";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        //TODO Тут бы еще и на администратора проверить
                        else if (message.UserId != roomToRename.OwnerId && 
                            userTryingToRename.RoleId.ToString() != StaticVars.ROLE_ADMIN)
                        {
                            result = $"Недостаточно прав для переименования комнаты с id = {roomToRename.Id}";
                            logger.LogInformation(result);
                            return Ok(result);
                        }

                        string newRoomName = renamedParts[1].Trim();
                        roomToRename.Name = newRoomName;
                        roomService.Update(roomToRename);
                        result = $"Обновлены данные о комнате с id = {roomToRename.Id}";
                    }
                    else if (messageParts.Length > 1 && messageParts[1] == "connect")
                    {
                        //Тут через || идут названия двух комнат.
                        string trimmedMessage = message.Text.Replace("//room", "").Replace("connect", "").Trim();
                        string[] connectedParts = trimmedMessage.Split(new string[] { " -l " }, StringSplitOptions.RemoveEmptyEntries);

                        //TODO Проверку ошибок бы всю в одно место, а то разбросало...
                        if (connectedParts.Length < 2)
                        {
                            result = "Неверный формат сообщения (не найден флаг -l)";
                            logger.LogInformation(result);
                            return Ok(result);
                        }

                        var roomToConnect = roomService.GetList().Where(
                            room => room.Name == connectedParts[0].Trim()).FirstOrDefault();
                        var userToConnect = userService.GetList().Where(
                            user => user.Name == connectedParts[1].Trim()).FirstOrDefault();

                        if (roomToConnect == null)
                        {
                            result = $"Неверное название комнаты";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        else if (userToConnect == null)
                        {
                            result = $"Неверное имя пользователя";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        //TODO Тут бы еще и на администратора проверить
                        else if (message.UserId != roomToConnect.OwnerId)
                        {
                            result = $"Недостаточно прав для добавления в комнату с id = {roomToConnect.Id}";
                            logger.LogInformation(result);
                            return Ok(result);
                        }



                        //roomToConnect.RoomUsers.Add(new RoomUser { Room = roomToConnect, User = userToConnect });
                        //roomToConnect.Users.Add(userToConnect);
                        //roomService.Update(roomToConnect);
                        roomUserService.Create(new RoomUser { RoomsId = roomToConnect.Id, Room = roomToConnect, User = userToConnect, UsersId = userToConnect.Id });
                        result = $"Обновлены данные о комнате с id = {roomToConnect.Id}";
                    }
                }
                else
                {
                    result = $"Сделано что-то другое";
                }
            }

            logger.LogInformation(result);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public ActionResult<Message> Edit(Message message)
        {
            messageService.Update(message);
            logger.LogInformation($"Обновлена запись сообщения с id = {message.Id}");
            return Ok($"Обновлена запись сообщения с id = {message.Id}");
        }

        [HttpDelete("{id}")]
        public ActionResult<Message> Delete(int id)
        {
            Message message = messageService.Get(id);
            if (message == null)
            {
                logger.LogError($"Нет сообщения с id = {id}");
                return NotFound();
            }

            messageService.Delete(id);
            logger.LogInformation($"Удалено сообщение с id = {id}");
            return Ok($"Удалено сообщение с id = {id}");
        }
    }
}