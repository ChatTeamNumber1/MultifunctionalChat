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
        private readonly IRepository<User> userService;
        private readonly ILogger<MessageController> logger;

        public MessageController(IRepository<Message> messageService, IRepository<Room> roomService, IRepository<User> userService, ILogger<MessageController> logger)
        {
            this.messageService = messageService;
            this.roomService = roomService;
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
                var userRoleId = userService.GetList().Where(
                            user => message.UserId == user.Id).FirstOrDefault().RoleId;
                if (userRoleId != 4)
                {
                    messageService.Create(message);
                    result = $"Сообщение с id = {message.Id} добавлено в общий список";
                }
                else
                    result = $"Пользователь с id = {message.UserId} забанен и не может писать сообщения";
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
                        
                        if (roomToRename == null)
                        {
                            result = $"Неверное название комнаты";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        //TODO Тут бы еще и на администратора проверить
                        else if (message.UserId != roomToRename.OwnerId)
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
                }
                if (messageParts[0] == "//user")
                {
                    if (messageParts.Length > 1 && messageParts[1] == "rename")
                    {
                        string trimmedMessage = message.Text.Replace("//user", "").Replace("rename", "").Trim();
                        string[] renamedParts = trimmedMessage.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

                        if (renamedParts.Length < 2)
                        {
                            result = "Неверный формат сообщения (не найдены ||)";
                            logger.LogInformation(result);
                            return Ok(result);
                        }

                        var userToRename = userService.GetList().Where(
                            user => user.Name == renamedParts[0].Trim()).FirstOrDefault();

                        var userRoleId = userService.GetList().Where(
                            user => message.UserId == user.Id).FirstOrDefault();

                        if (userToRename == null)
                        {
                            result = $"Неверное имя пользователя";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        if (userRoleId.RoleId == 1 || userRoleId.RoleId == 5)
                        {
                            string newUserName = renamedParts[1].Trim();
                            userToRename.Name = newUserName;
                            userService.Update(userToRename);
                            result = $"Обновлены данные о пользователе с id = {userToRename.Id}";
                        }
                        else
                        {
                            result = $"Недостаточно прав для переименования пользователя с id = {userToRename.Id}";
                            logger.LogInformation(result);
                            return Ok(result);
                        }                       
                    }
                    if (messageParts.Length > 1 && messageParts[1] == "moderator")
                    {
                        string trimmedMessage = message.Text.Replace("//user", "").Replace("moderator", "").Trim();
                        string[] actionModerator = trimmedMessage.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                        if (actionModerator.Length < 2)
                        {
                            result = "Неверный формат сообщения (не найдено действие)";
                            logger.LogInformation(result);
                            return Ok(result);
                        }

                        var userToModerator = userService.GetList().Where(
                            user => user.Login == actionModerator[0].Trim()).FirstOrDefault();

                        var userRoleId = userService.GetList().Where(
                            user => message.UserId == user.Id).FirstOrDefault();

                        if (userToModerator == null)
                        {
                            result = $"Неверный логин пользователя";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        else if (userRoleId.RoleId != 1)
                        {
                            result = $"Недостаточно прав для назначения модератора";
                            logger.LogInformation(result);
                            return Ok(result);

                        }
                        else if (actionModerator[1].Trim() == "n")
                        {
                            if (userToModerator.RoleId == 2)
                            {
                                result = $"Уже является модератором пользователь с id = {userToModerator.Id}";
                                logger.LogInformation(result);
                                return Ok(result);
                            }
                            userToModerator.RoleId = 2;
                            userService.Update(userToModerator);
                            result = $"Назначен модератором пользователь с id = {userToModerator.Id}";
                            logger.LogInformation(result);
                            return Ok(result);

                        }
                        else if (actionModerator[1].Trim() == "d")
                        {
                            if (userToModerator.RoleId == 3)
                            {
                                result = $"Не является модератором пользователь с id = {userToModerator.Id}";
                                logger.LogInformation(result);
                                return Ok(result);
                            }
                            userToModerator.RoleId = 3;
                            userService.Update(userToModerator);
                            result = $"Разжалован из модераторов пользователь с id = {userToModerator.Id}";
                            logger.LogInformation(result);
                            return Ok(result);

                        }
                    }
                    if (messageParts.Length > 1 && messageParts[1] == "ban")
                    {
                        string messageBan = message.Text.Replace("//user", "").Replace("ban", "").Trim();

                        var userToBan = userService.GetList().Where(
                            user => user.Login == messageBan.Trim()).FirstOrDefault();

                        var userRoleId = userService.GetList().Where(
                            user => message.UserId == user.Id).FirstOrDefault().RoleId;

                        if (userToBan == null)
                        {
                            result = $"Неверный логин пользователя";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        if (userRoleId == 1 || userRoleId == 2)
                        {
                            if (userToBan.RoleId == 4)
                            {
                                result = $"Уже забанен пользователь с id = {userToBan.Id}";
                                logger.LogInformation(result);
                                return Ok(result);
                            }
                            userToBan.RoleId = 4;
                            userService.Update(userToBan);
                            result = $"Забанен пользователь с id = {userToBan.Id}";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        else
                        {
                            result = $"Недостаточно прав для бана пользователя с id = {userToBan.Id}";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                    }
                    if (messageParts.Length > 1 && messageParts[1] == "pardon")
                    {
                        string messageUnban = message.Text.Replace("//user", "").Replace("pardon", "").Trim();

                        var userToUnban = userService.GetList().Where(
                            user => user.Login == messageUnban.Trim()).FirstOrDefault();

                        var userRoleId = userService.GetList().Where(
                            user => message.UserId == user.Id).FirstOrDefault().RoleId;

                        if (userToUnban == null)
                        {
                            result = $"Неверный логин пользователя";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        if (userRoleId == 1 || userRoleId == 2)
                        {
                            if (userToUnban.RoleId != 4)
                            {
                                result = $"Не забанен пользователь с id = {userToUnban.Id}";
                                logger.LogInformation(result);
                                return Ok(result);
                            }
                            userToUnban.RoleId = 3;
                            userService.Update(userToUnban);
                            result = $"Разбанен пользователь с id = {userToUnban.Id}";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        else
                        {
                            result = $"Недостаточно прав для разбана пользователя с id = {userToUnban.Id}";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
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