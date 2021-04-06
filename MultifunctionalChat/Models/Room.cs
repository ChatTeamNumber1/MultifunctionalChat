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
        //Общая (больше 2 людей) или приватная
        public bool IsPublic { get; set; }
        //Все пользователи (из таблицы RoomMembers)
        [NotMapped]
        public List<User> MembersList { get; set; }
    }
}