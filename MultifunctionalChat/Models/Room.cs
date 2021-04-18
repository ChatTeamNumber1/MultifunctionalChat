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
        //Общая (больше 2 людей) или приватная или чат-бот
        public bool IsPublic { get; set; }

        //Все пользователи
        public List<User> Users { get; set; }
        public List<RoomUser> RoomUsers { get; set; }
    }
}