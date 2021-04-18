using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultifunctionalChat.Models;
using MultifunctionalChat.Services;

namespace MultifunctionalChat.Controllers
{
    public class RoomController : Controller
    {
        private readonly IRepository<Room> _roomService;
        private readonly IRepository<RoomUser> _roomUserService;
        private readonly IRepository<User> _userService;
        private readonly ILogger<MessageController> _logger;

        public RoomController (IRepository<Room> roomService, IRepository<RoomUser> roomUserService, IRepository<User> userService, ILogger<MessageController> logger)
        {
            _logger = logger;
            _roomService = roomService;
            _roomUserService = roomUserService;
            _userService = userService;
        }
        public IActionResult Index(string id)
        {
            ViewBag.roomId = id; 
            
            var currentRoom = _roomService.GetList().Where(room => room.Id.ToString() == id).FirstOrDefault();
            ViewBag.room = currentRoom;

            var users = _userService.GetList();
            ViewBag.users = users;

            var currentUser = users.Where(us => us.Login == User.Identity.Name).FirstOrDefault();
            ViewBag.currentUser = currentUser;

            ViewBag.chatRooms = currentUser.Rooms;
            var roomUsers = _roomUserService.GetList().Where(ru => ru.RoomsId.ToString() == id);
            ViewBag.roomUsers = roomUsers;

            var Owner = users.Where(us => us.Id == currentRoom.OwnerId).FirstOrDefault();
            ViewBag.owner = Owner;

            //Если попали в какую-то комнату, где вас быть не должно...
            if (roomUsers.Where(ru => ru.User == currentUser).ToList().Count == 0 && 
                (currentUser.RoleId.ToString() == StaticVars.ROLE_USER || currentUser.RoleId.ToString() == StaticVars.ROLE_BANNED))
                return GetRoomsForUser();

            return View();
        }
        public ActionResult GetUsers(string id)
        {
            ViewBag.roomUsers = _roomUserService.GetList().Where(ru => ru.RoomsId.ToString() == id);
            return PartialView("GetUsers");
        }
        public ActionResult GetRoomsForUser()
        {
            var currentUser = _userService.GetList().Where(us => us.Login == User.Identity.Name).FirstOrDefault();
            ViewBag.chatRooms = currentUser.Rooms;

            return PartialView("GetRoomsForUser");
        }

        [HttpGet]
        public ActionResult<List<Room>> Get()
        {
            var roomList = _roomService.GetList();
            _logger.LogInformation("Выведен список комнат");
            return roomList;
        }

        [HttpGet("{id}")]
        public ActionResult<Room> Get(int id)
        {
            Room room = _roomService.Get(id);

            if (room == null)
            {
                _logger.LogError($"Комната c id = {id} не найдена");
                return NotFound();
            }

            _logger.LogInformation($"Выведена комната с id = {id}");
            return room;
        }

        [HttpPost]
        public ActionResult<Room> Add(Room newRoom)
        {
            _roomService.Create(newRoom);
            _logger.LogInformation($"Добавлена комната с id = {newRoom.Id}");
            return Ok($"Добавлена комната с id = {newRoom.Id}");
        }

        [HttpPut("{id}")]
        public ActionResult<Room> Edit(Room updatedRoom)
        {
            _roomService.Update(updatedRoom);
            _logger.LogInformation($"Обновлены данные о комнате с id = {updatedRoom.Id}");
            return Ok($"Обновлены данные о комнате с id = {updatedRoom.Id}");
        }

        [HttpDelete("{id}")]
        public ActionResult<Room> Delete(int id)
        {
            Room room = _roomService.Get(id);

            if (room == null)
            {
                _logger.LogError($"Комната c id = {id} не найдена");
                return NotFound();
            }

            _roomService.Delete(id);
            _logger.LogInformation($"Удалена комната с id = {id}");
            return Ok($"Удалена комната с id = {id}");
        }
    }
}
