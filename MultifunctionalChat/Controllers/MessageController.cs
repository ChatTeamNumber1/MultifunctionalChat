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
            this.roomUserService = roomUserService;
            this.roomService = roomService;
            this.userService = userService;
            this.logger = logger;

            foreach (RoomUser roomUser in roomUserService.GetList())
            {
                if (roomUser.Status != null && roomUser.BanStart != null && roomUser.BanInterval != null &&
                    roomUser.BanStart < DateTime.Now.AddMinutes(-(double)roomUser.BanInterval))
                {
                    roomUser.Status = null;
                    roomUser.BanInterval = null;
                    roomUser.BanStart = null;
                    roomUserService.Update(roomUser);
                }
            }
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
                var userTryingToPost = userService.Get(message.UserId);
                var currentRoom = roomService.Get(message.RoomId); 
                var roomUser = roomUserService.GetList().Where(
                    ru => ru.RoomsId == message.RoomId && ru.User.Id == message.UserId).FirstOrDefault();

                if (!currentRoom.Users.Contains(userTryingToPost) && roomUser.User.RoleId.ToString() == StaticVars.ROLE_USER || 
                    userTryingToPost.RoleId.ToString() == StaticVars.ROLE_BANNED ||
                    roomUser.Status != null && roomUser.User.RoleId.ToString() == StaticVars.ROLE_USER)
                {
                    result = "Вы не можете писать сообщения в этой комнате";
                    logger.LogInformation(result);
                    return NotFound(result);
                }
                messageService.Create(message);
                result = $"Сообщение с id = {message.Id} добавлено в общий список";
            }

            //Команды
            else
            {
                if (message.Text.StartsWith("//room create"))
                {
                    string messageTrimmed = message.Text.Replace("//room create", "").Trim();
                    string roomName = messageTrimmed;
                    string flag = null;
                    int roomNameLength = messageTrimmed.Length - 3;
                    bool flagExists = messageTrimmed.LastIndexOf(" -") == roomNameLength;

                    if (flagExists)
                    {
                        roomName = messageTrimmed.Substring(0, roomNameLength);
                        flag = messageTrimmed.Substring(messageTrimmed.LastIndexOf(" -"));
                    }

                    if (roomName == "")
                    {
                        result = "Неверный формат сообщения (отсутствует название комнаты)";
                        logger.LogError(result);
                        return BadRequest(result);
                    }

                    var currentUser = userService.GetList().Where(us => us.Login == User.Identity.Name).FirstOrDefault();
                    if (currentUser == null)
                    {
                        result = "Пользователь не найден, авторизация не пройдена";
                        logger.LogError(result);
                        return Unauthorized(result);
                    }

                    Room newRoom = new Room();
                    newRoom.Name = roomName;
                    newRoom.OwnerId = currentUser.Id;
                    newRoom.RoomUsers = new List<RoomUser>() { new RoomUser { UsersId = currentUser.Id, RoomsId = newRoom.Id } };
                    newRoom.IsPublic = (flag == null);

                    roomService.Create(newRoom);

                    result = $"Создана комната с id = {newRoom.Id}";
                }

                else if(message.Text.StartsWith("//room delete"))
                {
                    string roomName = message.Text.Replace("//room delete", "").Trim();
                    if (roomName == "")
                    {
                        result = "Неверный формат сообщения (отсутствует название комнаты)";
                        logger.LogError(result);
                        return BadRequest(result);
                    }

                    Room roomToDelete = roomService.GetList().Find(room => room.Name == roomName);
                    if (roomToDelete == null)
                    {
                        result = "Комната не найдена";
                        logger.LogError(result);
                        return NotFound(result);
                    }

                    var userTryingToDelete = userService.Get(message.UserId);
                    if (message.UserId != roomToDelete.OwnerId && userTryingToDelete.RoleId.ToString() != StaticVars.ROLE_ADMIN)
                    {
                        result = $"Недостаточно прав для удаления комнаты {roomToDelete.Name}";
                        logger.LogInformation(result);
                        return Forbid(result);
                    }

                    roomService.Delete(roomToDelete.Id);

                    result = $"Удалена комната с id = {roomToDelete.Id}";
                }

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
                            return NotFound(result);
                        }

                        var roomToRename = roomService.GetList().Where(
                            room => room.Name == renamedParts[0].Trim()).FirstOrDefault();
                        var userTryingToRename = userService.Get(message.UserId);

                        if (roomToRename == null)
                        {
                            result = $"Неверное название комнаты";
                            logger.LogInformation(result);
                            return NotFound(result);
                        }
                        else if (message.UserId != roomToRename.OwnerId && 
                            userTryingToRename.RoleId.ToString() != StaticVars.ROLE_ADMIN)
                        {
                            result = $"Недостаточно прав для переименования комнаты {roomToRename.Name}";
                            logger.LogInformation(result);
                            return NotFound(result);
                        }

                        string newRoomName = renamedParts[1].Trim();
                        roomToRename.Name = newRoomName;
                        roomService.Update(roomToRename);
                        result = $"Обновлены данные о комнате с id = {roomToRename.Id}";
                    }
                    else if (messageParts.Length > 1 && messageParts[1] == "connect")
                    {
                        return RoomConnect(message);
                    }
                    else if (messageParts.Length > 1 && messageParts[1] == "disconnect")
                    {
                        return RoomDisconnect(message);
                    }
                    else if (messageParts.Length > 1 && messageParts[1] == "mute")
                    {
                        return RoomMute(message);
                    }
                    else if (messageParts.Length > 1 && messageParts[1] == "speak")
                    {
                        return RoomSpeak(message);
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
                            user => user.Login == renamedParts[0].Trim()).FirstOrDefault();

                        var userRoleId = userService.GetList().Where(
                            user => message.UserId == user.Id).FirstOrDefault().RoleId;

                        if (userToRename == null)
                        {
                            result = $"Неверное имя пользователя";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        if (userRoleId == 1 || userRoleId == 5)
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
                            user => message.UserId == user.Id).FirstOrDefault().RoleId;

                        if (userToModerator == null)
                        {
                            result = $"Неверный логин пользователя";
                            logger.LogInformation(result);
                            return Ok(result);
                        }
                        else if (userRoleId != 1)
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
                            if (userToModerator.RoleId != 2)
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

        #region RoomCommands
        
        public ActionResult<Message> RoomConnect(Message message)
        {
            string result = "";
            string trimmedMessage = message.Text.Replace("//room", "").Replace("connect", "").Trim();
            string[] connectedParts = trimmedMessage.Split(new string[] { " -l " }, StringSplitOptions.RemoveEmptyEntries);

            if (connectedParts.Length < 2)
            {
                result = "Неверный формат сообщения (не найден флаг -l)";
                logger.LogInformation(result);
                return NotFound(result);
            }

            var roomToConnect = roomService.GetList().Where(
                room => room.Name == connectedParts[0].Trim()).FirstOrDefault();
            var userToConnect = userService.GetList().Where(
                user => user.Name == connectedParts[1].Trim()).FirstOrDefault();
            var userTryingToRunCommand = userService.Get(message.UserId);

            if (roomToConnect == null)
            {
                result = $"Неверное название комнаты";
                logger.LogInformation(result);
                return NotFound(result);
            }
            else if (userToConnect == null)
            {
                result = $"Неверное имя пользователя";
                logger.LogInformation(result);
                return NotFound(result);
            }
            else if (message.UserId != roomToConnect.OwnerId &&
                userTryingToRunCommand.RoleId.ToString() != StaticVars.ROLE_ADMIN)
            {
                result = $"Недостаточно прав для добавления в комнату {roomToConnect.Name}";
                logger.LogInformation(result);
                return NotFound(result);
            }
            else if (roomUserService.GetList().Where(ru => ru.RoomsId == roomToConnect.Id && ru.UsersId == userToConnect.Id) != null)
            {
                result = $"Пользователь и так состоит в комнате {roomToConnect.Name}";
                logger.LogInformation(result);
                return NotFound(result);
            }

            roomUserService.Create(new RoomUser { RoomsId = roomToConnect.Id, Room = roomToConnect, User = userToConnect, UsersId = userToConnect.Id });
            result = $"Обновлены данные о комнате с id = {roomToConnect.Id}";
            logger.LogInformation(result);
            return Ok(result);
        }

        public ActionResult<Message> RoomDisconnect(Message message)
        {
            string result = "";
            string trimmedMessage = message.Text.Replace("//room", "").Replace("disconnect", "").Trim();

            //Чисто дисконнект
            if (trimmedMessage == "")
            {
                RoomUser roomUser = roomUserService.GetList().Where(ru => ru.RoomsId == message.RoomId && ru.UsersId == message.UserId).FirstOrDefault();
                roomUserService.Delete(roomUser.Id);
                result = $"Вы вышли из комнаты {roomUser.Room.Name}";
            }
            else
            {
                string[] connectedParts = trimmedMessage.Split(new string[] { " -l ", " -m " }, StringSplitOptions.RemoveEmptyEntries);

                int loginPos = trimmedMessage.IndexOf(" -l ");
                int mutePos = trimmedMessage.IndexOf(" -m ");

                //Отсоединяемый пользователь
                RoomUser roomUser = roomUserService.GetList().Where(
                    ru => ru.Room.Name == connectedParts[0] && ru.UsersId == message.UserId).FirstOrDefault();
                if (roomUser == null)
                {
                    result = $"Неверное название комнаты";
                    logger.LogInformation(result);
                    return NotFound(result);
                }

                //Дисконнект с комнатой
                if (connectedParts.Length < 2)
                {
                    roomUserService.Delete(roomUser.Id);
                    result = $"Вы вышли из комнаты {roomUser.Room.Name}";
                }
                //Дисконнект другого юзера
                else
                {
                    var userTryingToRunCommand = userService.Get(message.UserId);

                    if (message.UserId != roomUser.Room.OwnerId &&
                        userTryingToRunCommand.RoleId.ToString() != StaticVars.ROLE_ADMIN &&
                        userTryingToRunCommand.RoleId.ToString() != StaticVars.ROLE_MODERATOR)
                    {
                        result = $"Недостаточно прав для удаления людей из комнаты {connectedParts[0]}";
                        logger.LogInformation(result);
                        return NotFound(result);
                    }

                    if (loginPos > 0 && (mutePos == -1 || mutePos > loginPos))
                    {
                        roomUser = roomUserService.GetList().Where(
                            ru => ru.Room.Name == connectedParts[0] && ru.User.Name == connectedParts[1]).FirstOrDefault();
                        if (roomUser == null)
                        {
                            result = $"Неверное имя пользователя";
                            logger.LogInformation(result);
                            return NotFound(result);
                        }

                        //Banned
                        roomUser.Status = 'B';
                        roomUser.BanInterval = null;
                        roomUser.BanStart = null;
                    }
                    if (mutePos > 0)
                    {
                        string muteTimeStr = connectedParts[1];
                        if (loginPos > 0 && loginPos < mutePos)
                        {
                            muteTimeStr = connectedParts[2];
                        }

                        if (!Int32.TryParse(muteTimeStr, out int muteTime))
                        {
                            result = $"Время блокировки - не число";
                            logger.LogInformation(result);
                            return NotFound(result);
                        }
                        //Banned
                        roomUser.Status = 'B';
                        roomUser.BanInterval = muteTime;
                        roomUser.BanStart = DateTime.Now;
                    }

                    roomUserService.Update(roomUser);
                    result = $"Пользователь {roomUser.User.Name} отключен";
                }
            }
            logger.LogInformation(result);
            return Ok(result);
        }
        
        public ActionResult<Message> RoomMute(Message message)
        {
            string result = "";
            string trimmedMessage = message.Text.Replace("//room", "").Replace("mute", "").Trim();
                        
            string[] connectedParts = trimmedMessage.Split(new string[] { "-l ", " -m " }, StringSplitOptions.RemoveEmptyEntries);
            int loginPos = trimmedMessage.IndexOf("-l ");
            int mutePos = trimmedMessage.IndexOf(" -m ");

            if (loginPos < 0)
            {
                result = "Неверный формат сообщения (не найден флаг -l)";
                logger.LogInformation(result);
                return NotFound(result);
            }
            RoomUser roomUser = roomUserService.GetList().Where(
                ru => ru.RoomsId == message.RoomId && ru.User.Name == connectedParts[0].Trim()).FirstOrDefault();
            var userTryingToRunCommand = userService.Get(message.UserId);

            if (roomUser == null)
            {
                result = $"Неверное имя пользователя";
                logger.LogInformation(result);
                return NotFound(result);
            }
            if (message.UserId != roomUser.Room.OwnerId &&
                userTryingToRunCommand.RoleId.ToString() != StaticVars.ROLE_ADMIN &&
                userTryingToRunCommand.RoleId.ToString() != StaticVars.ROLE_MODERATOR)
            {
                result = $"Недостаточно прав для блокировки людей из комнаты {roomUser.Room.Name}";
                logger.LogInformation(result);
                return NotFound(result);
            }

            //Muted
            roomUser.Status = 'M';
            roomUser.BanInterval = 10;
            roomUser.BanStart = DateTime.Now;

            if (mutePos > 0)
            {
                string muteTimeStr = connectedParts[1].Trim();

                if (!Int32.TryParse(muteTimeStr, out int muteTime))
                {
                    result = $"Время блокировки - не число";
                    logger.LogInformation(result);
                    return NotFound(result);
                }
                roomUser.BanInterval = muteTime;
                roomUser.BanStart = DateTime.Now;
            }

            roomUserService.Update(roomUser);
            result = $"Пользователь {roomUser.User.Name} Не может говорить";            
            logger.LogInformation(result);
            return Ok(result);
        }

        public ActionResult<Message> RoomSpeak(Message message)
        {
            string result = "";
            string trimmedMessage = message.Text.Replace("//room", "").Replace("speak", "").Trim();

            string[] connectedParts = trimmedMessage.Split(new string[] { "-l " }, StringSplitOptions.RemoveEmptyEntries);
            int loginPos = trimmedMessage.IndexOf("-l ");

            if (loginPos < 0)
            {
                result = "Неверный формат сообщения (не найден флаг -l)";
                logger.LogInformation(result);
                return NotFound(result);
            }
            if (connectedParts.Length > 1)
            {
                result = "Неверный формат сообщения (текст перед флагом -l)";
                logger.LogInformation(result);
                return NotFound(result);
            }
            RoomUser roomUser = roomUserService.GetList().Where(
                ru => ru.RoomsId == message.RoomId && ru.User.Name == connectedParts[0].Trim()).FirstOrDefault();
            var userTryingToRunCommand = userService.Get(message.UserId);

            if (roomUser == null)
            {
                result = $"Неверное имя пользователя";
                logger.LogInformation(result);
                return NotFound(result);
            }
            if (message.UserId != roomUser.Room.OwnerId &&
                userTryingToRunCommand.RoleId.ToString() != StaticVars.ROLE_ADMIN &&
                userTryingToRunCommand.RoleId.ToString() != StaticVars.ROLE_MODERATOR)
            {
                result = $"Недостаточно прав для разблокировки людей из комнаты {roomUser.Room.Name}";
                logger.LogInformation(result);
                return NotFound(result);
            }

            roomUser.Status = null;
            roomUser.BanInterval = null;
            roomUser.BanStart = null;
            roomUserService.Update(roomUser);
            result = $"Пользователь {roomUser.User.Name} снова может говорить";
            logger.LogInformation(result);
            return Ok(result);
        }

        #endregion
    }
}
