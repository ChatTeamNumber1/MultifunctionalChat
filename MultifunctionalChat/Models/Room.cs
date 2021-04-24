using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MultifunctionalChat.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //Владелец комнаты
        [ForeignKey("User")]
        public int OwnerId { get; set; }
        /// <summary>
        /// Тип комнаты (пустой, B, C)
        /// </summary>
        public char? Type { get; set; }

        //Все пользователи
        public List<User> Users { get; set; }
        public List<RoomUser> RoomUsers { get; set; }
    }
}