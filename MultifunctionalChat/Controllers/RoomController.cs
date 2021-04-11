using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MultifunctionalChat.Models;
using MultifunctionalChat.Services;

namespace MultifunctionalChat.Controllers
{
    public class RoomController : ControllerBase
    {
        private readonly IRepository<Room> _roomService;
        private readonly ILogger<RoomController> _logger;

        public RoomController (IRepository<Room> roomService, ILogger<RoomController> logger)
        {
            _roomService = roomService;
            _logger = logger;
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
