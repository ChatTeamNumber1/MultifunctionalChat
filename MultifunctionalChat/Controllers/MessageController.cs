﻿using System;
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

        private readonly List<Room> roomsList;
        private readonly List<User> usersList;
        private readonly List<RoomUser> roomUsersList;
        public MessageController(IRepository<Message> messageService, IRepository<Room> roomService,
            IRepository<User> userService, IRepository<RoomUser> roomUserService, ILogger<MessageController> logger)
        {
            this.messageService = messageService;
            this.roomUserService = roomUserService;
            this.roomService = roomService;
            this.userService = userService;
            this.logger = logger;
                        
            //Проверяем, не пора ли разбанить людей
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
            foreach (User user in userService.GetList())
            {
                if (user.RoleId.ToString() == StaticVars.ROLE_BANNED && user.BanStart != null && user.BanInterval != null &&
                    user.BanStart < DateTime.Now.AddMinutes(-(double)user.BanInterval))
                {
                    user.RoleId = Convert.ToInt32(StaticVars.ROLE_USER);
                    user.BanInterval = null;
                    user.BanStart = null;
                    userService.Update(user);
                }
            }

            roomsList = roomService.GetList();
            roomUsersList = roomUserService.GetList();
            usersList = userService.GetList();
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
            if (!message.Text.Trim().StartsWith("//"))
            {
                var userTryingToPost = userService.Get(message.UserId);
                var currentRoom = roomService.Get(message.RoomId); 
                var roomUser = roomUserService.GetList().Where(
                    ru => ru.RoomsId == message.RoomId && ru.User.Id == message.UserId).FirstOrDefault();

                if (userTryingToPost.RoleId.ToString() != StaticVars.ROLE_ADMIN &&  
                    userTryingToPost.RoleId.ToString() != StaticVars.ROLE_MODERATOR &&
                    (roomUser == null ||
                    !currentRoom.Users.Contains(userTryingToPost) && roomUser.User.RoleId.ToString() == StaticVars.ROLE_USER || 
                    userTryingToPost.RoleId.ToString() == StaticVars.ROLE_BANNED ||
                    roomUser.Status != null && roomUser.User.RoleId.ToString() == StaticVars.ROLE_USER))
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
                string[] messageParts = message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (messageParts[0] == "//room")
                {
                    if (messageParts.Length > 1 && messageParts[1] == "create")
                    {
                        return RoomCreate(message);
                    }                    
                    else if (messageParts.Length > 1 && messageParts[1] == "remove")
                    {
                        return RoomRemove(message);
                    }
                    else if (messageParts.Length > 1 && messageParts[1] == "rename")
                    {
                        return RoomRename(message);
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
                        return UserRename(message);
                    }
                    if (messageParts.Length > 1 && messageParts[1] == "moderator")
                    {
                        return UserModerator(message);
                    }
                    if (messageParts.Length > 1 && messageParts[1] == "ban")
                    {
                        return UserBan(message);
                    }
                    if (messageParts.Length > 1 && messageParts[1] == "pardon")
                    {
                        return UserPardon(message);
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

            User userTryingToRunCommand = userService.GetList().Where(us => us.Login == User.Identity.Name).FirstOrDefault();

            if (message == null)
            {
                logger.LogError($"Нет сообщения с id = {id}");
                return NotFound($"Нет сообщения с id = {id}");
            }
            if (userTryingToRunCommand == null ||
                userTryingToRunCommand.RoleId.ToString() == StaticVars.ROLE_BANNED ||
                userTryingToRunCommand.RoleId.ToString() == StaticVars.ROLE_USER && userTryingToRunCommand.Id != message.UserId)
            {
                logger.LogError($"Недостаточно прав для удаления сообщения");
                return NotFound($"Недостаточно прав для удаления сообщения");
            }

            messageService.Delete(id);
            logger.LogInformation($"Удалено сообщение с id = {id}");
            return Ok($"Удалено сообщение с id = {id}");
        }

        #region Проверка на наличие двух одноименных комнат/людей
        private string GetErrorMessageForDoubleRoomNames(string roomName)
        {
            string result = "";
            int roomCount = roomsList.Where(
                room => room.Name == roomName.Trim()).ToList().Count;
            if (roomCount > 1)
            {
                result = $"Комнат с таким именем много." + Environment.NewLine +
                    "Укажите рядом с именем id комнаты." + Environment.NewLine +
                    "Например, //room rename Имя(id) || Новое_имя";
            }
            return result;
        }
        private bool CheckForDoubleRoomUserNames(string roomName, string userName)
        {
            int roomUsersCount = roomUsersList.Where(
                roomUser => roomUser.Room.Name == roomName.Trim() &&
                roomUser.User.Name == userName.Trim()).ToList().Count;
            return (roomUsersCount > 1);
        }

        private string GetErrorMessageForDoubleUserNames(string userName)
        {
            string result = "";
            int userCount = usersList.Where(
                user => user.Name == userName.Trim()).ToList().Count;
            if (userCount > 1)
            {
                result = $"Пользователей с таким именем много." + Environment.NewLine +
                    "Укажите рядом с именем id пользователя." + Environment.NewLine +
                    "Например, //user rename Имя(id) || Новое_имя";
            }
            return result;
        }

        private Room GetRoomFromStringWithId(string text)
        {
            Room room = roomsList.Where(r => r.Name == text.Trim()).FirstOrDefault();

            if (room == null && text.Contains("("))
            {
                string[] roomParts = text.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

                if (roomParts.Length > 1)
                    room = roomsList.Where(r => r.Name == roomParts[1].Trim()).FirstOrDefault();
            }

            return room;
        }
        
        private User GetUserFromStringWithId(string text)
        {
            User user = usersList.Where(
                user => user.Name == text.Trim()).FirstOrDefault();

            if (user == null && text.Contains("("))
            {
                string[] userParts = text.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

                if (userParts.Length > 1)
                    user = usersList.Where(
                        us => us.Id.ToString() == userParts[1].Trim()).FirstOrDefault();
            }

            return user;
        }
        #endregion

        #region UserCommands

        public ActionResult<Message> UserRename(Message message)
        {
            string result = "";
            string trimmedMessage = message.Text.Replace("//user", "").Replace("rename", "").Trim();
            string[] renamedParts = trimmedMessage.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

            if (renamedParts.Length < 2)
            {
                result = "Неверный формат сообщения (не найдены ||)";
                logger.LogInformation(result);
                return NotFound(result);
            }

            result = GetErrorMessageForDoubleUserNames(renamedParts[0]);
            if (result != "")
            {
                logger.LogInformation(result);
                return NotFound(result);
            }

            var userToRename = GetUserFromStringWithId(renamedParts[0]);
            if (userToRename == null)
            {
                result = $"Неверное имя пользователя";
                logger.LogInformation(result);
                return Unauthorized(result);
            }

            var userRoleId = usersList.Where(
                user => message.UserId == user.Id).FirstOrDefault().RoleId;
            if (userRoleId.ToString() != StaticVars.ROLE_ADMIN)
            {
                result = $"Недостаточно прав для переименования пользователя {userToRename.Name}";
                logger.LogInformation(result);
                return Unauthorized(result);
            }

            string newUserName = renamedParts[1].Trim();
            userToRename.Name = newUserName;
            userService.Update(userToRename);
            result = $"Обновлены данные о пользователе с id = {userToRename.Id}";   
            logger.LogInformation(result);
            return Ok(result);
        }

        public ActionResult<Message> UserBan(Message message)
        {
            string result = "";
            string messageBan = message.Text.Replace("//user", "").Replace("ban", "").Trim();
            string[] connectedParts = messageBan.Split(new string[] { " -m " }, StringSplitOptions.RemoveEmptyEntries);

            result = GetErrorMessageForDoubleUserNames(connectedParts[0]);
            if (result != "")
            {
                logger.LogInformation(result);
                return NotFound(result);
            }

            var userToBan = GetUserFromStringWithId(connectedParts[0]);
            if (userToBan == null)
            {
                result = $"Неверный логин пользователя";
                logger.LogInformation(result);
                return NotFound(result);
            }

            var userRoleId = usersList.Where(
                user => message.UserId == user.Id).FirstOrDefault().RoleId;
            if (userRoleId.ToString() != StaticVars.ROLE_ADMIN && userRoleId.ToString() != StaticVars.ROLE_MODERATOR)
            {
                result = $"Недостаточно прав для бана пользователя с id = {userToBan.Id}";
                logger.LogInformation(result);
                return NotFound(result);
            } 
            if (userToBan.RoleId == Convert.ToInt32(StaticVars.ROLE_BANNED))
            {
                result = $"Уже забанен пользователь с id = {userToBan.Id}";
                logger.LogInformation(result);
                return NotFound(result);
            }

            userToBan.BanInterval = null;
            userToBan.BanStart = null;
            userToBan.RoleId = Convert.ToInt32(StaticVars.ROLE_BANNED);
            if (connectedParts.Length > 1)
            {
                string muteTimeStr = connectedParts[1];
                if (!Int32.TryParse(muteTimeStr, out int muteTime))
                {
                    result = $"Время блокировки - не число";
                    logger.LogInformation(result);
                    return NotFound(result);
                }

                userToBan.BanInterval = muteTime;
                userToBan.BanStart = DateTime.Now;
            }

            userService.Update(userToBan);
            result = $"Забанен пользователь с id = {userToBan.Id}";
            logger.LogInformation(result);
            return Ok(result);
        }
        
        public ActionResult<Message> UserPardon(Message message)
        {
            string messageUnban = message.Text.Replace("//user", "").Replace("pardon", "").Trim();
            string result = GetErrorMessageForDoubleUserNames(messageUnban);
            if (result != "")
            {
                logger.LogInformation(result);
                return NotFound(result);
            }

            var userRoleId = userService.Get(message.UserId).RoleId;
            var userToUnban = GetUserFromStringWithId(messageUnban);
            if (userToUnban == null)
            {
                result = $"Неверный логин пользователя";
                logger.LogInformation(result);
                return NotFound(result);
            }
            else if (userToUnban.RoleId.ToString() != StaticVars.ROLE_BANNED)
            {
                result = $"Не забанен пользователь с id = {userToUnban.Id}";
                logger.LogInformation(result);
                return NotFound(result);
            }
            else if (userRoleId.ToString() != StaticVars.ROLE_ADMIN &&
                userRoleId.ToString() != StaticVars.ROLE_MODERATOR)
            {
                result = $"Недостаточно прав для разбана пользователя {userToUnban.Name}";
                logger.LogInformation(result);
                return NotFound(result);
            }

            userToUnban.RoleId = Convert.ToInt32(StaticVars.ROLE_USER);
            userToUnban.BanInterval = null;
            userToUnban.BanStart = null;
            userService.Update(userToUnban);
            result = $"Разбанен пользователь с id = {userToUnban.Id}";

            logger.LogInformation(result);
            return Ok(result);
        }
        
        public ActionResult<Message> UserModerator(Message message)
        {
            string result;
            string trimmedMessage = message.Text.Replace("//user", "").Replace("moderator", "").Trim();
            string[] actionModerator = trimmedMessage.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (actionModerator.Length < 2)
            {
                result = "Неверный формат сообщения (не найдено действие)";
                logger.LogInformation(result);
                return NotFound(result);
            }

            result = GetErrorMessageForDoubleUserNames(actionModerator[0]);
            if (result != "")
            {
                logger.LogInformation(result);
                return NotFound(result);
            }

            var userToModerator = GetUserFromStringWithId(actionModerator[0]);
            var userRoleId = userService.Get(message.UserId).RoleId;

            if (userToModerator == null)
            {
                result = $"Неверный логин пользователя";
                logger.LogInformation(result);
                return NotFound(result);
            }
            else if (userRoleId.ToString() != StaticVars.ROLE_ADMIN)
            {
                result = $"Недостаточно прав для назначения модератора";
                logger.LogInformation(result);
                return NotFound(result);
            }
            else if (actionModerator[1].Trim() == "-n")
            {
                if (userToModerator.RoleId.ToString() == StaticVars.ROLE_MODERATOR)
                {
                    result = $"Уже является модератором пользователь {userToModerator.Name}";
                    logger.LogInformation(result);
                    return NotFound(result);
                }

                userToModerator.RoleId = Convert.ToInt32(StaticVars.ROLE_MODERATOR);
                userService.Update(userToModerator);
                result = $"Назначен модератором пользователь с id = {userToModerator.Id}";
            }
            else if (actionModerator[1].Trim() == "-d")
            {
                if (userToModerator.RoleId.ToString() != StaticVars.ROLE_MODERATOR)
                {
                    result = $"Не является модератором пользователь {userToModerator.Name}";
                    logger.LogInformation(result);
                    return NotFound(result);
                }

                userToModerator.RoleId = Convert.ToInt32(StaticVars.ROLE_USER);
                userService.Update(userToModerator);
                result = $"Разжалован из модераторов пользователь с id = {userToModerator.Id}";
            }

            logger.LogInformation(result);
            return Ok(result);
        }

        #endregion

        #region RoomCommands

        public ActionResult<Message> RoomCreate(Message message)
        {
            string result;
            string trimmedMessage = message.Text.Replace("//room", "").Replace("create", "").Trim();

            string roomName = trimmedMessage;
            string flag = null;
            int roomNameLength = trimmedMessage.Length - 3;
            bool flagExists = trimmedMessage.LastIndexOf(" -") == roomNameLength;

            if (flagExists)
            {
                roomName = trimmedMessage.Substring(0, roomNameLength);
                flag = trimmedMessage.Substring(trimmedMessage.LastIndexOf(" -"));
            }


            var userTryingToRunCommand = userService.Get(message.UserId);

            if (roomName == "")
            {
                result = "Неверный формат сообщения (отсутствует название комнаты)";
                logger.LogError(result);
                return BadRequest(result);
            }
            else if (userTryingToRunCommand == null)
            {
                result = $"Неверное имя пользователя";
                logger.LogInformation(result);
                return Unauthorized(result);
            }
            else if (userTryingToRunCommand.RoleId.ToString() == StaticVars.ROLE_BANNED)
            {
                result = $"Вы забанены. Недостаточно прав для создания комнат";
                logger.LogInformation(result);
                return Unauthorized(result);
            }


            Room newRoom = new Room { Name = roomName, OwnerId = message.UserId };
            newRoom.RoomUsers = new List<RoomUser>() { new RoomUser { UsersId = message.UserId, RoomsId = newRoom.Id } };
            if (flag == " -b")
                newRoom.Type = 'B';
            if (flag == " -c")
                newRoom.Type = 'C';

            roomService.Create(newRoom);

            result = $"Создана комната с id = {newRoom.Id}";
            logger.LogInformation(result);
            return Ok(result);
        }

        public ActionResult<Message> RoomRemove(Message message)
        {
            string result = "";
            string trimmedMessage = message.Text.Replace("//room", "").Replace("remove", "").Trim();
            string roomName = trimmedMessage;

            if (roomName == "")
            {
                result = "Неверный формат сообщения (отсутствует название комнаты)";
                logger.LogError(result);
                return NotFound(result);
            }

            Room roomToDelete = roomsList.Find(room => room.Name == roomName);
            if (roomToDelete == null)
            {
                result = "Комната не найдена";
                logger.LogError(result);
                return NotFound(result);
            }

            var userTryingToDelete = userService.Get(message.UserId);
            if (message.UserId != roomToDelete.OwnerId &&
                userTryingToDelete.RoleId.ToString() != StaticVars.ROLE_ADMIN)
            {
                result = $"Недостаточно прав для удаления комнаты {roomToDelete.Name}";
                logger.LogInformation(result);
                return NotFound(result);
            }

            roomService.Delete(roomToDelete.Id);

            result = $"Удалена комната с id = {roomToDelete.Id}";
            logger.LogInformation(result);
            return Ok(result);
        }

        public ActionResult<Message> RoomRename(Message message)
        {
            string result = "";
            string trimmedMessage = message.Text.Replace("//room", "").Replace("rename", "").Trim();
            //Тут через || идут названия двух комнат.
            string[] renamedParts = trimmedMessage.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

            if (renamedParts.Length < 2)
            {
                result = "Неверный формат сообщения (не найдены ||)";
                logger.LogInformation(result);
                return NotFound(result);
            }

            var roomToRename = roomsList.Where(
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
            logger.LogInformation(result);
            return Ok(result);
        }

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
            
            result = GetErrorMessageForDoubleUserNames(connectedParts[1]);
            if (result != "")
            {
                logger.LogInformation(result);
                return NotFound(result);
            }

            var userToConnect = GetUserFromStringWithId(connectedParts[1]);            
            var roomToConnect = roomService.GetList().Where(
                room => room.Name == connectedParts[0].Trim()).FirstOrDefault();
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
            else if (roomToConnect.Type != null && roomToConnect.Type == 'B')
            {
                result = $"Нельзя добавлять людей в комнату с ботом";
                logger.LogInformation(result);
                return NotFound(result);
            }
            else if (roomToConnect.Type != null && roomToConnect.Type == 'C' && roomToConnect.RoomUsers.Count >= 2)
            {
                result = $"Нельзя добавлять людей в приватную комнату. Вас уже много";
                logger.LogInformation(result);
                return NotFound(result);
            }
            else if (roomUserService.GetList().Where(ru => ru.RoomsId == roomToConnect.Id && ru.UsersId == userToConnect.Id).FirstOrDefault() != null)
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

                    result = GetErrorMessageForDoubleUserNames(connectedParts[1]);
                    if (result != "")
                    {
                        logger.LogInformation(result);
                        return NotFound(result);
                    }

                    var userToDisconnect = GetUserFromStringWithId(connectedParts[1]);
                    if (loginPos > 0 && (mutePos == -1 || mutePos > loginPos))
                    {
                        roomUser = roomUserService.GetList().Where(
                            ru => ru.Room.Name == connectedParts[0] && ru.User.Id == userToDisconnect.Id).FirstOrDefault();
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

            result = GetErrorMessageForDoubleUserNames(connectedParts[0]);
            if (result != "")
            {
                logger.LogInformation(result);
                return NotFound(result);
            }

            var userToMute = GetUserFromStringWithId(connectedParts[0]);
            RoomUser roomUser = roomUserService.GetList().Where(
                ru => ru.RoomsId == message.RoomId && ru.User.Id == userToMute.Id).FirstOrDefault();
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
           
            if (!trimmedMessage.Contains("-l "))
            {
                result = "Неверный формат сообщения (не найден флаг -l)";
                logger.LogInformation(result);
                return NotFound(result);
            }
            else if (connectedParts.Length > 1)
            {
                result = "Неверный формат сообщения (текст перед флагом -l)";
                logger.LogInformation(result);
                return NotFound(result);
            }

            var roomUsers = roomUsersList.Where(
                ru => ru.RoomsId == message.RoomId && ru.User.Name == connectedParts[0].Trim()).ToList();
            if (roomUsers.Count > 1)
            {
                result = "В комнате больше одного пользователя с таким именем. Укажите в скобках его id";
                logger.LogInformation(result);
                return NotFound(result);
            }

            //Если пользователей было больше одного, ищем id в строке
            var userToMute = GetUserFromStringWithId(connectedParts[0]); 
            roomUsers = roomUsersList.Where(
                ru => ru.RoomsId == message.RoomId && ru.User.Id == userToMute.Id).ToList();
            if (roomUsers == null || roomUsers.Count == 0)
            {
                result = $"Неверное имя пользователя";
                logger.LogInformation(result);
                return NotFound(result);
            }


            RoomUser roomUser = roomUsers.FirstOrDefault();
            var userTryingToRunCommand = userService.Get(message.UserId);

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
